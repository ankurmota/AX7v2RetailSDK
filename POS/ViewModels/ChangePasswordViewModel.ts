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

    export interface IChangePasswordViewModelOptions {
        staffId: string;
    }

    /**
     * Represents the change password view model.
     */
    export class ChangePasswordViewModel extends ViewModelBase {

        private _staffId: string;
        private _oldPassword: Observable<string>;
        private _newPassword: Observable<string>;
        private _confirmedNewPassword: Observable<string>;
        private _processing: Observable<boolean>;

        constructor(options?: IChangePasswordViewModelOptions) {
            super();

            if (!ObjectExtensions.isNullOrUndefined(options) && !StringExtensions.isNullOrWhitespace(options.staffId)) {
                this._staffId = options.staffId;
            } else if (!StringExtensions.isNullOrWhitespace(Session.instance.CurrentEmployee.StaffId)) {
                this._staffId = Commerce.Session.instance.CurrentEmployee.StaffId;
            } else {
                throw "ChangePasswordViewModel::ctor could not properly initialize the staff id, which is a required value.";
            }

            this._oldPassword = ko.observable("");
            this._newPassword = ko.observable("");
            this._confirmedNewPassword = ko.observable("");
            this._processing = ko.observable(false);
        }

        /**
         * Changes the password.
         */
        public changePassword(): void {

            var errors: Model.Entities.Error[] = this.validateFields();
            if (ArrayExtensions.hasElements(errors)) {
                NotificationHandler.displayClientErrors(errors, "string_6804");
                return;
            }

            this._processing(true);

            var options: Operations.IChangePasswordOperationOptions = {
                staffId: this._staffId,
                oldPassword: this._oldPassword(),
                newPassword: this._newPassword()
            };

            Operations.OperationsManager.instance.runOperation(
                Operations.RetailOperation.ChangePassword, options).done((operationResult: Operations.IOperationResult): void => {
                    if (Session.instance.isLoggedOn) {
                        ViewModelAdapter.navigate("HomeView");
                    } else {
                        ViewModelAdapter.navigate("LoginView");
                    }
                }).fail((errors: Proxy.Entities.Error[]): void => {
                    NotificationHandler.displayClientErrors(errors);
                }).always((): void => {
                    this._processing(false);
                });
        }

        /**
         * Clear the password text fields when the user clicks cancel.
         */
        public clearPasswordFields(): void {
            this._oldPassword("");
            this._newPassword("");
            this._confirmedNewPassword("");
        }

        /**
         * Cancel the operation and navigate back.
         */
        public cancelHandler(): void {
            ViewModelAdapter.navigateBack();
        }

        /**
         * Validates the password text fields when the user clicks ok.
         * @returns {Proxy.Entities.Error[]} The errors.
         */
        private validateFields(): Proxy.Entities.Error[] {
            var errors: Proxy.Entities.Error[] = [];

            if (StringExtensions.isEmptyOrWhitespace(this._oldPassword())
                || StringExtensions.isEmptyOrWhitespace(this._newPassword())
                || StringExtensions.isEmptyOrWhitespace(this._confirmedNewPassword())) {
                errors.push(new Proxy.Entities.Error(ErrorTypeEnum.CHANGE_PASSWORD_DETAILS_NOT_SPECIFIED));
            }

            if (StringExtensions.compare(this._newPassword(), this._confirmedNewPassword())) {
                errors.push(new Proxy.Entities.Error(ErrorTypeEnum.NEW_PASSWORD_AND_CONFIRMATION_NOT_MATCHING_ERROR));
            }

            return errors;
        }
    }
}