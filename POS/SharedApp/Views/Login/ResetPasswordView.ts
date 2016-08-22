/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
/// <reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    /**
     * Represents the reset password view controller.
     */
    export class ResetPasswordViewController extends ViewControllerBase {

        // state properties
        public commonHeaderData: Commerce.Controls.CommonHeaderData;
        public _viewModel: Commerce.ViewModels.LoginViewModel;
        public indeterminateWaitVisible: Observable<boolean>;
        public operatorId: Observable<string>;
        public newPassword: Observable<string>;
        public confirmedNewPassword: Observable<string>;
        public requireChangeAfterUse: Observable<boolean>;

        constructor() {
            super(true);

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this._viewModel = new Commerce.ViewModels.LoginViewModel();

            this.operatorId = ko.observable("");
            this.newPassword = ko.observable("");
            this.confirmedNewPassword = ko.observable("");
            this.indeterminateWaitVisible = ko.observable(false);
            this.requireChangeAfterUse = ko.observable(true);

            // Load Common Header 
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_6815"));
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_6809"));
        }

        /**
         * Called when view is shown.
         */
        public onShown(): void {
            this.clearPasswordFields();
        }

        /**
         * Toggles the require change after use flag.
         * @param {any} eventInfo The event info.
         * @param {boolean} requireChangeAfterUse The new require change after use flag value.
         */
        public toggleRequireChange(eventInfo: any, requireChangeAfterUse: boolean): void {
            this.requireChangeAfterUse(requireChangeAfterUse);
        }

        /**
         * Resets the password.
         */
        public resetPassword(): void {
            this.indeterminateWaitVisible(true);

            var errors: Model.Entities.Error[] = this.validateFields();
            if (ArrayExtensions.hasElements(errors)) {
                this.indeterminateWaitVisible(false);
                NotificationHandler.displayClientErrors(errors, "string_6804");

                return;
            }

            this._viewModel.resetPassword(this.operatorId(), this.newPassword(), this.requireChangeAfterUse())
                .done(() => {
                    this.indeterminateWaitVisible(false);
                    Commerce.ViewModelAdapter.navigate("HomeView");
                }).fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        /**
         * Clear the password text fields when the user clicks cancel.
         */
        private clearPasswordFields(): void {
            this.operatorId("");
            this.newPassword("");
            this.confirmedNewPassword("");
        }

        /**
         * Validates the password text fields when the user clicks ok.
         *
         * @returns {Model.Entities.Error[]} The errors.
         */
        private validateFields(): Model.Entities.Error[] {
            var errors: Model.Entities.Error[] = [];

            if (StringExtensions.compare(this.operatorId(), Commerce.Session.instance.CurrentEmployee.StaffId) === 0) {
                // Select an operator ID that is different than the current operator ID.
                errors.push(new Model.Entities.Error(ErrorTypeEnum.RESET_PASSWORD_CURRENT_EMPLOYEE));
            }

            if (StringExtensions.isEmptyOrWhitespace(this.operatorId())
                || StringExtensions.isEmptyOrWhitespace(this.newPassword())
                || StringExtensions.isEmptyOrWhitespace(this.confirmedNewPassword())) {
                // Enter the operator ID and the new password. Then confirm the new password.
                errors.push(new Model.Entities.Error(ErrorTypeEnum.RESET_PASSWORD_DETAILS_NOT_SPECIFIED));
            }

            if (StringExtensions.compare(this.newPassword(), this.confirmedNewPassword())) {
                // The new password and the confirmation password don't match.
                errors.push(new Model.Entities.Error(ErrorTypeEnum.NEW_PASSWORD_AND_CONFIRMATION_NOT_MATCHING_ERROR));
            }

            return errors;
        }
    }
}