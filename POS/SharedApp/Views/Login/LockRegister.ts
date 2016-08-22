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
///<reference path='LoginViewHelper.ts'/>

module Commerce.ViewControllers {
    "use strict";

    /**
     * Options for lock register view.
     */
    export interface ILockRegisterViewControllerOptions {
        /**
         * The operator id.
         */
        OperatorId: string;
    }

    export class LockRegisterViewController extends ViewControllerBase {

        public viewModel: Commerce.ViewModels.LoginViewModel;

        public store: Observable<string>;
        public operatorId: Observable<string>;
        public loading: Observable<boolean>;

        private _isPortraitOrientation: Observable<boolean>;
        private _disableUnlockButton: Computed<boolean>;
        private _isSigningIn: boolean;

        /**
         * @constructor
         */
        constructor(options: ILockRegisterViewControllerOptions) {
            super(true);

            this.loading = ko.observable(false);

            this.viewModel = new Commerce.ViewModels.LoginViewModel();

            this.store = ko.observable(ApplicationStorage.getItem(ApplicationStorageIDs.STORE_ID_KEY));
            this.operatorId = ko.observable(options.OperatorId);
            this.viewModel.password("");

            this._isPortraitOrientation = ko.observable(false);

            this._disableUnlockButton = ko.computed(() => {
                var requiredInputMissing: boolean = StringExtensions.isEmptyOrWhitespace(this.viewModel.password());

                return this.loading() || requiredInputMissing;
            }, this);
        }

        /**
         * Called when view is shown.
         */
        public onShown(): void {
            // Enable extended logon with barcode
            Commerce.Peripherals.instance.barcodeScanner.enableAsync(
                (barcode: string) => {
                    this.unlockRegister(barcode, Authentication.Providers.CommerceUserAuthenticationProvider.EXTENDEDLOGON_BARCODE_GRANT_TYPE);
                });

            // Enable extended logon with MSR
            Commerce.Peripherals.instance.magneticStripeReader.enableAsync(
                (cardInfo: Proxy.Entities.CardInfo) => {
                    this.unlockRegister(cardInfo.CardNumber, Authentication.Providers.CommerceUserAuthenticationProvider.EXTENDEDLOGON_MSR_GRANT_TYPE);
                });
        }

        /**
         * Called when view is hidden.
         */
        public onHidden(): void {
            // Disable barcode scanner.
            Commerce.Peripherals.instance.barcodeScanner.disableAsync();

            // Disable MSR.
            Commerce.Peripherals.instance.magneticStripeReader.disableAsync();
        }

        /**
         * Occurs when the element of the page is created.
         *
         * @param {HTMLElement} element DOM element.
         */
        public onCreated(element: HTMLElement): void {
            super.onCreated(element);
            this._isPortraitOrientation(Commerce.ApplicationContext.Instance.tillLayoutProxy.orientation === Model.Entities.Orientation.Portrait);
            // Detect orientation to handle different background images
            Commerce.ApplicationContext.Instance.tillLayoutProxy.addOrientationChangedHandler(element, (args: string) => {
                this._isPortraitOrientation(args === Model.Entities.Orientation.Portrait);
            });
        }

        /**
         * Switch the user.
         */
        public switchUser(): void {
            Commerce.ViewModelAdapter.navigate("LoginView");
        }

        /**
         * Unlock register UI handler.
         */
        public unlockRegisterButtonClick(): void {
            this.unlockRegister();
        }

        /**
         * Unlocks the register.
         * @param {string} [extendedCredentials] Extended credentials, e.g. barcode.
         * @param {string} [grantType] The grant type.
         */
        private unlockRegister(extendedCredentials?: string, grantType?: string): void {
            if (this._isSigningIn) {
                return;
            }
            this.loading(true);
            this._isSigningIn = true;
            this.viewModel.unlockRegister(this.operatorId(), extendedCredentials, grantType)
                .done(() => {
                    this.loading(false);
                    Commerce.ViewModelAdapter.navigate(Commerce.ApplicationContext.Instance.tillLayoutProxy.startView); // Use AX default start screen for this
                    this._isSigningIn = false;
                }).fail((errors: Model.Entities.Error[]) => {
                    this.loading(false);

                    if (LoginViewHelper.isPasswordExpired(errors)) {
                        // User must change password
                        NotificationHandler.displayClientErrors(errors)
                            .always(() => {
                                var options: IChangePasswordViewOptions = {
                                    staffId: this.operatorId()
                                };

                                Commerce.ViewModelAdapter.navigate("ChangePasswordView", options);
                                this._isSigningIn = false;
                            });
                    } else if (LoginViewHelper.isPasswordRequired(errors)) {
                        LoginViewHelper.handlePasswordRequiredDialog()
                            .onAny((password: string, dialogResult: DialogResult) => {
                                this._isSigningIn = false;
                                if (dialogResult === DialogResult.OK) {
                                    this.viewModel.password(password);
                                    this.unlockRegister(extendedCredentials, grantType);
                                }
                            });
                    } else {
                        NotificationHandler.displayClientErrors(errors, Commerce.ViewModelAdapter.getResourceString("string_506")) // Sign-in error
                            .always(() => this._isSigningIn = false);
                    }
                });
        }
    }
}