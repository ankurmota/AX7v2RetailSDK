/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.ViewControllers {
    export class LoginViewHelper {

        /**
         * Checks whether the error set contains the error which indicated that the password is required.
         * @param {Proxy.Entities.Error[]} errors The array of errors.
         * @return {boolean} True if the password is required otherwise false.
         */
        public static isPasswordRequired(errors: Proxy.Entities.Error[]): boolean {
            return ErrorHelper.hasError(errors,
                Proxy.Entities.SecurityErrors[Proxy.Entities.SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_PasswordRequired]);
        }

        /**
         * Checks whether the error set contains the error which indicated that the password is expired.
         * @param {Proxy.Entities.Error[]} errors The array of errors.
         * @return {boolean} True if the password is expired, otherwise false.
         */
        public static isPasswordExpired(errors: Proxy.Entities.Error[]): boolean {
            return ErrorHelper.hasError(errors,
                ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_USERPASSWORDEXPIRED.serverErrorCode);
        }

        /**
         * Handles the dialog which requests for password.
         * @return {IAsyncDialogResult<string>} The dialog result.
         */
        public static handlePasswordRequiredDialog(): IAsyncDialogResult<string> {
            var textInputDialog: Controls.TextInputDialog = new Controls.TextInputDialog();
            textInputDialog.title(ViewModelAdapter.getResourceString("string_520")); // Provide the password to sign in
            return textInputDialog.show({ type: "password", labelResx: "string_521" /* Password */ }, true /* hideOnResult */);
        }
    }
}