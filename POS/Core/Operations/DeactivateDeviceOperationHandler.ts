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
     * Options passed to the DeactivateDevice operation.
     */
    export interface IDeactivateDeviceOperationOption extends IOperationOptions {
    }

    /**
     * Handler for the DeactivateDevice operation.
     */
    export class DeactivateDeviceOperationHandler extends OperationHandlerBase {
        /**
         * Executes the DeactivateDevice operation.
         * @param {IDeactivateDeviceOperationOption} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IDeactivateDeviceOperationOption): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || {};

            return this.authenticationManager.deactivateDeviceAsync().done(() => {
                new Activities.ClearMerchantInformationActivity().execute().always(() => {
                    ApplicationStorage.clear();
                    Session.instance.resetSession();
                    ApplicationContext.Instance = null;
                    Config.retailServerUrl = StringExtensions.EMPTY;
                    Utilities.OfflineHelper.stopOffline();
                    ViewModelAdapter.navigate(Helpers.DeviceActivationHelper.DEVICE_ACTIVATION_GET_STARTED_VIEW_NAME);
                });
            });
        }
    }
}
