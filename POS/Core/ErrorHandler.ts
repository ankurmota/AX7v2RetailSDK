/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Session.ts'/>

module Commerce {
    "use strict";

    /**
     * Error Handlers 
     */
    export class ErrorHandler {
        private static SERVICE_UNAVAILABLE_ERROR_CODE = "string_29278";

        /**
         * Error handler for HTTP 401 Unauthorized response.
         */
        public static authenticationErrorHandler(errors: Model.Entities.Error[]): void {
            // use first error to decide handling action
            var error: Model.Entities.Error = (errors || [])[0];

            if (ObjectExtensions.isNullOrUndefined(error)) {
                return;
            }

            var errorMessage: string = ErrorHelper.getErrorMessages(errors);

            if (error.commerceExceptionType === Proxy.ErrorHandling.CommerceExceptionTypes.USER_AUTHENTICATION_EXCEPTION_TYPE) {
                // Log user authentication errors as warnings since this is expected behavior
                RetailLogger.coreLogUserAuthenticationRetailServerResponse(error.ErrorCode, errorMessage);
            } else if (error.ErrorCode !== ErrorHandler.SERVICE_UNAVAILABLE_ERROR_CODE || Commerce.Session.instance.connectionStatus === ConnectionStatusType.Online) {
                // Do not log service unavailable error when mPOS is in offline.
                RetailLogger.coreLogOriginalUnauthorizedRetailServerResponse(error.ErrorCode, errorMessage);
            }

            switch (error.commerceExceptionType) {
                case Proxy.ErrorHandling.CommerceExceptionTypes.USER_AUTHENTICATION_EXCEPTION_TYPE:
                    ErrorHandler.handleUserAuthenticationException();
                    break;

                case Proxy.ErrorHandling.CommerceExceptionTypes.DEVICE_AUTHENTICATION_EXCEPTION_TYPE:
                    // when device authentication error happens, clear device token information and go back to device authentication
                    Authentication.AuthenticationProviderManager.instance.logoff(Authentication.AuthenticationProviderResourceType.DEVICE);

                    // clear the local storage so that we could redo the device activation and initial data sync flows.
                    Commerce.ApplicationStorage.clear();
                    Utilities.OfflineHelper.stopOffline();
                    ErrorHandler.navigateToView(Helpers.DeviceActivationHelper.GUIDED_ACTIVATION_VIEW_NAME, <Proxy.Entities.IActivationParameters>{ errors: errors });
                    break;
            }
        }

        public static handleUserAuthenticationException(): void {
            // When user authentication error happens, log off and return to login page as long as we aren't on one of the sign in pages already
            if (!Commerce.ViewModelAdapter.isInView(Commerce.ViewModelAdapter.LOGIN_VIEW)
                && !Commerce.ViewModelAdapter.isInView("LockRegister")
                && !Commerce.ViewModelAdapter.isInView("ManagerOverrideView")
                && !Commerce.ViewModelAdapter.isInView("ChangePasswordView")) {
                Utilities.LogonHelper.logoff();

                ErrorHandler.navigateToView(Commerce.ViewModelAdapter.LOGIN_VIEW);
            }
        }

        private static navigateToView<T>(viewName: string, viewParameters?: T): void {
            // Do not redirect if device activation view is active.
            if (!Helpers.DeviceActivationHelper.isInDeviceActivationProcess()) {
                Commerce.ViewModelAdapter.navigate(viewName, viewParameters);
            }
        }
    }
}