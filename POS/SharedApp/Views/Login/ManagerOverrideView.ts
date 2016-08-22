/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class ManagerOverrideViewController extends ViewControllerBase {
        public ViewModel: Commerce.ViewModels.ManagerOverrideViewModel;

        private _operationId: number;
        private _managerOverrideAsyncResult: VoidAsyncResult;

        private _isPortraitOrientation: Observable<boolean>;
        private _orientationChangedHandler: any;

        private _isPasswordFocused: Observable<boolean>;
        private _disableSignInButton: Computed<boolean>;

        /**
         * Create an instance of ManagerOverrideViewController
         *
         * @constructor
         */
        constructor(options: any) {
            super(true);

            this.ViewModel = new Commerce.ViewModels.ManagerOverrideViewModel();

            this._operationId = options.OperationId;
            this._managerOverrideAsyncResult = options.ManagerOverrideResult;

            this._isPortraitOrientation = ko.observable(false);
            this._isPasswordFocused = ko.observable(false);

            this._disableSignInButton = ko.computed(() => {
                var requiredInputMissing: boolean =
                    StringExtensions.isEmptyOrWhitespace(this.ViewModel.operatorId()) || StringExtensions.isEmptyOrWhitespace(this.ViewModel.password());

                return this.ViewModel.loading() || requiredInputMissing;
            }, this);
        }

        /**
         * Try to sign In.
         */
        private signInHandler() {
            this.ViewModel.managerLogOn(this._operationId)
                .done(() => {
                    // navigate back
                    Commerce.ViewModelAdapter.navigateBack(this,
                        () => {
                            this.ViewModel.loading(false);
                            this._managerOverrideAsyncResult.resolve();
                        });
                }).fail((errors: Model.Entities.Error[]) => {
                    this.ViewModel.loading(false);

                    // No need to reject the managerOverrideAsyncResult. Might be a typo that caused a login fail.
                    // Display the error and let the UI sit there.
                    Commerce.NotificationHandler.displayClientErrors(errors).always((() => {
                        // Focus is set to the password box after error message is displayed.
                        this._isPasswordFocused(true);
                    }).bind(this));
                });
        }


        /**
         * Cancel the manager override operation.
         */
        private cancelHandler() {
            // navigate back
            Commerce.ViewModelAdapter.navigateBack(this,
                () => {
                    this.ViewModel.loading(false);
                    this._managerOverrideAsyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.MANAGER_OVERRIDE_CANCELED_ERROR)]);
                });
        }

        /**
         * Occurs when the element of the page is created.
         *
         * @param {HTMLElement} element DOM element.
         */
        public onCreated(element: HTMLElement) {
            super.onCreated(element);
            this._isPortraitOrientation(Commerce.ApplicationContext.Instance.tillLayoutProxy.orientation === Model.Entities.Orientation.Portrait);
            // Detect orientation to handle different background images
            Commerce.ApplicationContext.Instance.tillLayoutProxy.addOrientationChangedHandler(element, (args) => {
                this._isPortraitOrientation(args === Model.Entities.Orientation.Portrait);
            });
        }
    }

    export class ManagerOverrideOperationHandler {

        /**
        * Execute the operation.
        * 
        * @param {any} callerContext - The callback context.
        * @param {number} operationId - the operation id.
        * @param {VoidAsyncResult} managerOverrideResult - the result of the manager override operation.
        * @return {IVoidAsyncResult} The async result.
        */
        public static execute(callerContext: any, operationId: number, managerOverrideResult: VoidAsyncResult): IVoidAsyncResult {
            Commerce.ViewModelAdapter.navigate("ManagerOverrideView", { OperationId: operationId, ManagerOverrideResult: managerOverrideResult });

            return VoidAsyncResult.createResolved();
        }
    }
}
