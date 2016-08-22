/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='OperationHandlerBase.ts' />

module Commerce.Operations {
    "use strict";

    /**
     * Options passed to the ResetPassword operation.
     */
    export interface IResetPasswordOperationOptions extends IOperationOptions {
        targetUserId: string;
        newPassword: string;
        changePassword: boolean;
    }

    /**
     * Handler for the ResetPassword operation.
     */
    export class ResetPasswordOperationHandler extends OperationHandlerBase {
        /**
         * Executes the ResetPassword operation.
         * @param {IResetPasswordOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IResetPasswordOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { targetUserId: undefined, newPassword: undefined, changePassword: undefined };

            return this.authenticationManager.resetPassword({
                userId: options.targetUserId,
                newPassword: options.newPassword,
                mustChangePasswordAtNextLogOn: options.changePassword
            });
        }
    }
}
