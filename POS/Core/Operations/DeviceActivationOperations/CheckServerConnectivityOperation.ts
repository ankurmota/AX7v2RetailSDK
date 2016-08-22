/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path='DeviceActivationOperationBase.ts' />

module Commerce.Operations {
    "use strict";

    export class CheckServerConnectivityOperation extends DeviceActivationOperationBase {
        constructor(stateActivity: Model.Entities.IDeviceActivationState) {
            super(stateActivity);
        }

        public static operationProcessImpl(serviceUrl: string): IVoidAsyncResult {
            var asyncResult: VoidAsyncResult = new VoidAsyncResult();
            var authManager: Model.Managers.IAuthenticationManager = Model.Managers.Factory.getManager
                <Model.Managers.IAuthenticationManager>(Model.Managers.IAuthenticationManagerName);

            authManager.checkServerHealthAsync(serviceUrl)
                .done((healthCheckStatuses: Model.Entities.IHealthCheck[]) => {
                    var parser: HealthCheckParser = new HealthCheckParser();

                    if (parser.isValidEntity(healthCheckStatuses)) {
                        asyncResult.resolve();
                    } else {
                        var healthCheckResponseError: Model.Entities.Error = new Model.Entities.Error(
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_HEALTH_CHECK_FAILED.serverErrorCode);

                        asyncResult.reject([healthCheckResponseError]);
                    }
                }).fail((errors: Model.Entities.Error[]) => {
                    authManager.checkServerConnectivityAsync(serviceUrl)
                        .done(() => {

                            var healthCheckResponseError: Model.Entities.Error = new Model.Entities.Error(
                                ErrorTypeEnum.MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_HEALTH_CHECK_FAILED.serverErrorCode);

                            asyncResult.reject([healthCheckResponseError]);
                        }).fail((errors: Model.Entities.Error[]) => {

                            if (ArrayExtensions.hasElements(errors) && errors[0].ErrorCode === ErrorTypeEnum.SERVICE_UNAVAILABLE) {
                                errors[0] = new Model.Entities.Error(
                                    ErrorTypeEnum.MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_ERROR.serverErrorCode);
                            }

                            asyncResult.reject(errors);
                        });
                });

            return asyncResult;
        }

        /**
         * Gets the name of the activation operation.
         * @returns {string} The name of the operation.
         */
        public operationName(): string {
            return Helpers.DeviceActivationHelper.CHECK_SERVER_CONNECTIVITY_OPERATION_NAME;
        }

        /**
         * Gets the message status for user when the operation is executing asynchronous function.
         * @returns {string} The message status.
         */
        public processingStatusName(): string {
            return ViewModelAdapter.getResourceString("string_8041");
        }

        /**
         * Gets the message for user when the operation failed executing asynchronous function.
         * @returns {string} The message error status.
         */
        public errorStatusName(): string {
            return ViewModelAdapter.getResourceString("string_8042");
        }

        /**
         * The asynchronous execution of the operation.
         * @returns {() => IVoidAsyncResult} The async result.
         */
        public operationProcess(): () => IVoidAsyncResult {
            return () => {
                return CheckServerConnectivityOperation.operationProcessImpl(this.stateOperation.serviceUrl);
            };
        }

        /**
         * Evaluate if the state entity has non nullable properties needed for this operation to be executed
         * and the server url has the https protocol.
         * @returns {() => IVoidAsyncResult} The async result.
         */
        public validateState(): () => IVoidAsyncResult {
            return () => {
                var asyncResult: VoidAsyncResult = new VoidAsyncResult();

                super.validateState()().done(() => {
                    var serverUrl: string = this.stateOperation.serviceUrl;
                    if (StringExtensions.isNullOrWhitespace(serverUrl)) {
                        asyncResult.reject(this.createRejectResponseForMissingProperties("serviceUrl"));
                    } else if (!UrlHelper.isHttpsProtocol(serverUrl)) {
                        asyncResult.reject([
                            new Model.Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_POS_SERVER_URL_NOT_HTTPS)
                        ]);
                    } else {
                        asyncResult.resolve();
                    }
                }).fail((errors: Model.Entities.Error[]) => {
                    asyncResult.reject(errors);
                });

                return asyncResult;
            };
        }
    }
}