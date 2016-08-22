/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../IAuthenticationManager.ts'/>

module Commerce.Model.Managers.RetailServer {
    "use strict";

    import Common = Proxy.Common;
    import Context = Proxy.Context;

    export class AuthenticateManager implements Commerce.Model.Managers.IAuthenticationManager {
        private _commerceContext: Proxy.CommerceContext = null;
        private commerceAuthenticationContext: Context.CommerceAuthenticationContext = null;

        constructor(commerceContext: Proxy.CommerceContext, commerceAuthenticationContext: Context.CommerceAuthenticationContext) {
            this._commerceContext = commerceContext;
            this.commerceAuthenticationContext = commerceAuthenticationContext;
        }

        /**
         * Check if server is accessible for client to connect.
         * @param {string} url The server URL.
         * @returns {IVoidAsyncResult} The async result.
         */
        checkServerConnectivityAsync(url: string): IVoidAsyncResult {
            var asyncResult: VoidAsyncResult = new VoidAsyncResult(null);
            var regex: RegExp = new RegExp("\/+$");
            url = url.replace(regex, StringExtensions.EMPTY);

            OData.request({
                requestUri: url,
                method: Common.HttpVerbs.GET,
                data: undefined // keep undefined to reduce the amount of headers datajs sends
            }, (data: any) => {
                    asyncResult.resolve();
                }, (error: any) => {
                    if (!ObjectExtensions.isNullOrUndefined(error) && !ObjectExtensions.isNullOrUndefined(error.response) &&
                        !ObjectExtensions.isNullOrUndefined(error.response.statusCode)) {

                        var statusCode: number = error.response.statusCode;

                        // When user enters 'localhost' as server URL, it may returns error
                        // but actually status code is success.
                        if (Common.HttpStatusCodes.isSuccessful(statusCode)) {
                            asyncResult.resolve();
                        } else {
                            asyncResult.reject(Context.ErrorParser.parseErrorMessage(error.response));
                        }
                    } else if (!ObjectExtensions.isNullOrUndefined(error) && !ObjectExtensions.isNullOrUndefined(error.response)) {
                        asyncResult.reject(Context.ErrorParser.parseErrorMessage(error.response));
                    } else {
                        asyncResult.reject(Context.ErrorParser.parseErrorMessage(error));
                    }
                },
                OData.jsonHandler);

            return asyncResult;
        }

        /**
         * Check the server health URL.
         * @param {string} url The server URL.
         * @returns {IAsyncResult<Entities.IHealthCheck[]>} The async result.
         */
        public checkServerHealthAsync(url: string): IAsyncResult<Entities.IHealthCheck[]> {
            var asyncResult: AsyncResult<Entities.IHealthCheck[]> = new AsyncResult<Entities.IHealthCheck[]>();

            var serverUrl: string = UrlHelper.getServerRootUrl(url);
            var healthCheckUrl: string = serverUrl + HealthCheckParser.HEALTH_CHECK_URL;

            var request: Common.IXmlHttpRequest = {
                requestUri: healthCheckUrl,
                body: StringExtensions.EMPTY
            };

            var successMethod: (response: Common.IXmlHttpResponse) => void = (response: Common.IXmlHttpResponse) => {
                var xmlResponse: string = response.body;
                var parser: HealthCheckParser = new HealthCheckParser();
                var healthCheckEntities: Entities.IHealthCheck[] = parser.parse(xmlResponse);
                asyncResult.resolve(healthCheckEntities);
            };

            var errorMethod: (errorDetails: Common.IXmlHttpError) => void = (errorDetails: Common.IXmlHttpError) => {
                asyncResult.reject(Context.ErrorParser.parseErrorMessage(errorDetails.response));
            };

            Common.XmlHttpRequestHelper.executeRequest(request, successMethod, errorMethod);
            return asyncResult;
        }

        /**
         * Get Retail Server URL.
         * @param {string} locatorUrl Locator Service URL.
         * @param {string} tenantId Tenant Identifier.
         * @return {IAsyncResult<Locator.Model.Entities.ServiceEndpoint>} The async result.
         */
        public getRetailServerUrl(locatorUrl: string, tenantId: string): IAsyncResult<Locator.Model.Entities.ServiceEndpoint[]> {
            var request: Common.IDataServiceRequest = new Context.LocatorContext(locatorUrl).getRetailServerUrl(tenantId);
            return request.execute<Locator.Model.Entities.ServiceEndpoint[]>();
        }

        /**
         * Activate a device.
         * @param {string} deviceNumber The device unique number.
         * @param {string} terminalId The terminal identifier.
         * @param {string} deviceId The device identifier.
         * @param {boolean} forceActivate True if user want to force activation, false otherwise.
         * @return {IAsyncResult<Entities.Device>} The async result.
         */
        public activateDeviceAsync(deviceNumber: string, terminalId: string, deviceId: string, forceActivate: boolean): IAsyncResult<Entities.Device> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations()
                .activateDevice(deviceNumber, terminalId, deviceId, forceActivate, Host.instance.application.getApplicationType());
            return request.execute<Entities.DeviceActivationResult>();
        }

        /**
         * Deactivate a device.
         * @return {IVoidAsyncResult} The async result.
         */
        public deactivateDeviceAsync(): IVoidAsyncResult {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations()
                .deactivateDevice(NumberSequence.GetNextTransactionId());
            return request.execute();
        }

        /**
         * Requests a user token.
         * @param {Entities.Authentication.ILogonRequest} logonRequest The logon request object.
         * @return {IAsyncResult<string>} The async result.
         */
        public requestUserToken(logonRequest: Entities.Authentication.ILogonRequest): IAsyncResult<Entities.Authentication.ICommerceToken> {
            var request: Common.IDataServiceRequest = this.commerceAuthenticationContext.token(logonRequest);
            return request.execute<Entities.Authentication.ICommerceToken>();
        }

        /**
         * Enrolls user credentials.
         * @param {Entities.Authentication.IEnrollRequest} request The enroll request object.
         * @return {IVoidAsyncResult} The async result.
         */
        public enrollUserCredentials(request: Entities.Authentication.IEnrollRequest): IVoidAsyncResult {
            return this.commerceAuthenticationContext.enrollUserCredentials(request).execute();
        }

        /**
         * Disenrolls user credentials.
         * @param {Entities.Authentication.IDisenrollRequest} request The disenroll request object.
         * @return {IVoidAsyncResult} The async result.
         */
        public disenrollUserCredentials(request: Entities.Authentication.IDisenrollRequest): IVoidAsyncResult {
            return this.commerceAuthenticationContext.disenrollUserCredentials(request).execute();
        }

        /**
         * Creates a hardware station token.
         * @return {IAsyncResult<Entities.CreateHardwareStationTokenResult>} The async result.
         */
        public createHardwareStationToken(): IAsyncResult<Entities.CreateHardwareStationTokenResult> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().createHardwareStationToken();
            return request.execute<Entities.CreateHardwareStationTokenResult>();
        }

        /**
         * Change Password.
         * @param {Entities.Authentication.IChangePasswordRequest} request The change password request object.
         * @return {IVoidAsyncResult} The async result.
         */
        public changePassword(request: Entities.Authentication.IChangePasswordRequest): IVoidAsyncResult {
            return this.commerceAuthenticationContext.changePassword(request).execute();
        }

        /**
         * Reset Password.
         * @param {Entities.Authentication.IResetPasswordRequest} request The reset password request object.
         * @return {IVoidAsyncResult} The async result.
         */
        public resetPassword(request: Entities.Authentication.IResetPasswordRequest): IVoidAsyncResult {
            return this.commerceAuthenticationContext.resetPassword(request).execute();
        }

        /**
         * Starts a new session.
         */
        public startSessionAsync(): IVoidAsyncResult {
            return this._commerceContext.storeOperations().startSession(NumberSequence.GetNextTransactionId()).execute();
        }

        /**
         * Ends the session.
         */
        public endSessionAsync(): IVoidAsyncResult {
            return this._commerceContext.storeOperations().endSession(NumberSequence.GetNextTransactionId()).execute();
        }
    }
}