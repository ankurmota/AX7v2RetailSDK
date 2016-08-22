/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Utilities {
    "use strict";

    /**
     * Helper class for logon operations.
     */
    export class LogonHelper {
        /**
         * Performs a resource owned password logon.
         * @param {string} userId the user identifier.
         * @param {string} password the user's password.
         * @param {number} operationId the logon key.
         * @param {boolean} doNotUpdateSession the logon type.
         * @param {string} credentials the credentials, e.g. extended logon.
         * @param {string} grantType the grant type identifier.
         * @returns {IVoidAsyncResult} the result completion promise.
         */
        public static resourceOwnedPasswordLogon(
            userId: string,
            password: string,
            operationId?: number,
            doNotUpdateSession?: boolean,
            credentials?: string,
            grantType?: string): IVoidAsyncResult {
            var logonRequestParameters: { [parameterName: string]: string; } = {
                OperationId: (operationId || "").toString(),
                Credentials: credentials,
                GrantType: grantType
            };

            var authenticationProvider: Commerce.Authentication.IResourceOwnerPasswordGrantAuthenticationProvider;
            authenticationProvider = Commerce.Authentication.AuthenticationProviderManager.instance.getResourceOwnerPasswordGrantProvider(
                Commerce.Authentication.AuthenticationProviderResourceType.USER);

            return new AsyncQueue().enqueue((): IVoidAsyncResult => {
                return authenticationProvider.logon(userId, password, [], logonRequestParameters);
            }).enqueue((): IVoidAsyncResult => {
                if (doNotUpdateSession === true) {
                    return VoidAsyncResult.createResolved();
                }

                return LogonHelper.updateSessionWithUserTokenInformation();
            }).run();
        }

        /**
         * Register the session with user information from available token.
         * @returns {IVoidAsyncResult} promise for result completion.
         */
        public static updateSessionWithUserTokenInformation(): IVoidAsyncResult {
            return Commerce.Authentication.AuthenticationProviderManager.instance.acquireToken(
                Commerce.Authentication.AuthenticationProviderResourceType.USER).done((token: Authentication.IAuthenticationToken): void => {

                    // if no token, it means user is not logged in
                    if (ObjectExtensions.isNullOrUndefined(token)) {
                        Commerce.Session.instance.CurrentEmployee = null;
                        Commerce.Session.instance.isSessionStateValid = true;
                    } else {
                        var employee: Commerce.Model.Entities.Employee = new Model.Entities.EmployeeClass();
                        employee.Name = token.name;
                        employee.Permissions = {
                            Roles: []
                        };

                        Commerce.Session.instance.CurrentEmployee = employee;
                        Commerce.Session.instance.isSessionStateValid = true;
                    }
                });
        }

        /**
         * Performs the end session call.
         * @returns {IVoidAsyncResult} the result completion promise.
         */
        public static endSession(): IVoidAsyncResult {
            // End session only if device is activated.
            if (ApplicationContext.Instance.isDeviceActivated) {
                return Model.Managers.Factory
                    .getManager<Model.Managers.IAuthenticationManager>(Model.Managers.IAuthenticationManagerName)
                    .endSessionAsync()
                    .recoverOnFailure((): IVoidAsyncResult => {
                        // ignore server failures to avoid leaving user blocked
                        return VoidAsyncResult.createResolved();
                    });
            } else {
                return VoidAsyncResult.createResolved();
            }
        }

        /**
         * Performs the logoff against the user authentication provider.
         * @returns {IVoidAsyncResult} the result completion promise.
         */
        public static logoffAuthenticationProvider(): IVoidAsyncResult {
            var result: VoidAsyncResult = new VoidAsyncResult();

            // ignore server failures as client calls logoff just to inform server of intent
            // all tokens will be cleared locally
            Commerce.Authentication.AuthenticationProviderManager.instance.logoff(
                Commerce.Authentication.AuthenticationProviderResourceType.USER).always((): void => {
                    LogonHelper.updateSessionWithUserTokenInformation().always((): void => {
                        result.resolve();
                    });
                });

            return result;
        }

        /**
         * Performs the end session call and the logoff against the user authentication provider.
         * @returns {IVoidAsyncResult} the result completion promise.
         */
        public static logoff(): IVoidAsyncResult {
            return new AsyncQueue()
                .enqueue((): IVoidAsyncResult => LogonHelper.endSession())
                .enqueue((): IVoidAsyncResult => LogonHelper.logoffAuthenticationProvider())
                .run();
        }
    }
}