/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce.Model.Managers {
    "use strict";

    
    export var IAuthenticationManagerName: string = "IAuthenticationManager";
    

    export interface IAuthenticationManager {
        /**
         * Check if server is accessible for client to connect.
         * @param {string} url The server URL.
         * @returns {IVoidAsyncResult} The async result.
         */
        checkServerConnectivityAsync(url: string): IVoidAsyncResult;

        /**
         * Check the server health URL.
         * @param {string} url The server URL.
         * @returns {IAsyncResult<Entities.IHealthCheck[]>} The async result.
         */
        checkServerHealthAsync(url: string): IAsyncResult<Entities.IHealthCheck[]>;

        /**
         * Get Retail Server URL.
         * @param {string} locatorUrl Locator Service URL.
         * @param {string} tenantId Tenant Identifier.
         * @return {IAsyncResult<Locator.Model.Entities.ServiceEndpoint>} The async result.
         */
        getRetailServerUrl(locatorUrl: string, tenantId: string): IAsyncResult<Locator.Model.Entities.ServiceEndpoint[]>;

        /**
         * Activate a device.
         * @param {string} deviceNumber The device unique number.
         * @param {string} terminalId The terminal identifier.
         * @param {string} deviceId The device identifier.
         * @param {boolean} forceActivate True if user want to force activation, false otherwise.
         * @return {IAsyncResult<Entities.Device>} The async result.
         */
        activateDeviceAsync(deviceNumber: string, terminalId: string, deviceId: string, forceActivate: boolean): IAsyncResult<Entities.Device>;

        /**
         * Deactivate a device.
         * @return {IVoidAsyncResult} The async result.
         */
        deactivateDeviceAsync(): IVoidAsyncResult;

        /**
         * Requests a user token.
         * @param {Entities.Authentication.ILogonRequest} logonRequest The logon request object.
         * @return {IAsyncResult<string>} The async result.
         */
        requestUserToken(logonRequest: Entities.Authentication.ILogonRequest): IAsyncResult<Entities.Authentication.ICommerceToken>;

        /**
         * Enrolls user credentials.
         * @param {Entities.Authentication.ILogonRequest} request The enroll request object.
         * @return {IVoidAsyncResult} The async result.
         */
        enrollUserCredentials(request: Entities.Authentication.IEnrollRequest): IVoidAsyncResult;

        /**
         * Disenrolls user credentials.
         * @param {Entities.Authentication.ILogonRequest} request The disenroll request object.
         * @return {IVoidAsyncResult} The async result.
         */
        disenrollUserCredentials(request: Entities.Authentication.IDisenrollRequest): IVoidAsyncResult;

        /**
         * Creates a hardware station token.
         * @return {IAsyncResult<Entities.CreateHardwareStationTokenResult>} The async result.
         */
        createHardwareStationToken(): IAsyncResult<Entities.CreateHardwareStationTokenResult>;

        /**
         * Change Password.
         * @param {Entities.Authentication.IChangePasswordRequest} request The change password request object.
         * @return {IVoidAsyncResult} The async result.
         */
        changePassword(request: Entities.Authentication.IChangePasswordRequest): IVoidAsyncResult;

        /**
         * Reset Password.
         * @param {Entities.Authentication.IResetPasswordRequest} request The reset password request object.
         * @return {IVoidAsyncResult} The async result.
         */
        resetPassword(equest: Entities.Authentication.IResetPasswordRequest): IVoidAsyncResult;

        /**
         * Starts a new session.
         */
        startSessionAsync(): IVoidAsyncResult;

        /**
         * Ends the session.
         */
        endSessionAsync(): IVoidAsyncResult;
    }
}