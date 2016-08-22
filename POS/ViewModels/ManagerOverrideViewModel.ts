/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    export class ManagerOverrideViewModel extends ViewModelBase {

        public loading: Observable<boolean>;
        public storeId: Observable<string>;
        public operatorId: Observable<string>;
        public password: Observable<string>;

        constructor(options?: any) {
            super();
            this.loading = ko.observable(false);
            this.password = ko.observable("");
            this.storeId = ko.observable(ApplicationStorage.getItem(ApplicationStorageIDs.STORE_ID_KEY));
            this.operatorId = ko.observable("");
        }

        /**
         * Logs on the manager.
         *
         * @param {number} operationId - The operation id.
         * @return {IVoidAsyncResult} The async result.
         */
        public managerLogOn(operationId: number): IVoidAsyncResult {
            this.loading(true);

            if (StringExtensions.isEmptyOrWhitespace(this.operatorId()) || StringExtensions.isEmptyOrWhitespace(this.password())) {
                var retailError = new Model.Entities.Error(ErrorTypeEnum.OPERATOR_ID_PASSWORD_NOT_SPECIFIED);
                return AsyncResult.createRejected([retailError]);
            }

            var deviceToken = ApplicationStorage.getItem(ApplicationStorageIDs.DEVICE_TOKEN_KEY);
            var deviceId = ApplicationStorage.getItem(ApplicationStorageIDs.DEVICE_ID_KEY);
            var employeeToken: Authentication.IAuthenticationToken = null;

            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    // get current employee token and store it so we can revert to this user latter on
                    return Authentication.AuthenticationProviderManager.instance.acquireToken(Authentication.AuthenticationProviderResourceType.USER)
                        .done((token: Authentication.IAuthenticationToken) => {
                            employeeToken = token;
                        });
                }).enqueue(() => {
                    // get the token for the new user (manager override)
                    return Utilities.LogonHelper.resourceOwnedPasswordLogon(this.operatorId(), this.password(), operationId, true);
                }).enqueue(() => {
                    var result = this.operatorManager.getEmployeeAsync(this.operatorId());

                    return result.done((employee: Proxy.Entities.Employee) => {
                        Commerce.Operations.OperationsManager.instance.managerInformation = employee;
                        Commerce.Operations.OperationsManager.instance.employeeToken = employeeToken;
                    });
                });

            return asyncQueue.run();
        }
    }
}