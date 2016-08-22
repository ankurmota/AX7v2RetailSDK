/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    /**
     * Options passed to the change password view.
     */
    export interface IChangePasswordViewOptions {
        staffId: string;
    }

    /**
     * Represents the change password view controller.
     */
    export class ChangePasswordViewController extends ViewControllerBase {

        private _viewModel: ViewModels.ChangePasswordViewModel;

        constructor(options?: IChangePasswordViewOptions) {
            super(true);

            this._viewModel = new Commerce.ViewModels.ChangePasswordViewModel(options);
        }

        /**
         * Called when view is shown.
         */
        public onShown(): void {
            this._viewModel.clearPasswordFields();
        }
    }
}