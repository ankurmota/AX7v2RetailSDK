/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../ViewControllerBase.ts'/>
///<reference path='LoginViewHelper.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class LoginViewController extends ViewControllerBase {
        public commonHeaderData: Controls.CommonHeaderData;
        public viewModel: Commerce.ViewModels.LoginViewModel;
        public shiftViewModel: Commerce.ViewModels.ShiftViewModel;

        public operatorList: ObservableArray<any>;
        public selectedOperator: Observable<Commerce.Model.Entities.Employee>;
        public operatorId: Observable<string>;
        public password: Observable<string>;
        public isOperatorIdTextVisible: Computed<boolean>;
        public isOperatorIdSelectVisible: Computed<boolean>;
        public disableInput: Computed<boolean>;
        public disableSignInButton: Computed<boolean>;
        public indeterminateWaitVisible: Observable<boolean>;
        public signInButton: Observable<HTMLElement>;

        private _showList: Observable<boolean>;
        private applicationVersion: Observable<string>;

        private firstTimeUsageVisible: Observable<boolean>;

        private _passwordExpiryDialog: Controls.MessageDialog;

        private _orientationMediaQuery: MediaQueryList;
        private _isPortraitOrientation: Observable<boolean>;
        private _orientationChangedHandler: any;
        private _isSigningIn: boolean = false;

        /**
         * Create an instance of LoginViewModel
         * @constructor
         */
        constructor(options: any) {
            super(true);

            this.viewModel = new Commerce.ViewModels.LoginViewModel();
            this.shiftViewModel = new Commerce.ViewModels.ShiftViewModel();

            this.firstTimeUsageVisible = ko.observable(false);
            this.signInButton = ko.observable(null);
            this.indeterminateWaitVisible = ko.observable(false);

            this.operatorList = ko.observableArray([]);
            this.selectedOperator = ko.observable(new Model.Entities.EmployeeClass({ StaffId: ("") }));

            this.operatorId = ko.observable(Commerce.Config.demoModeStaffId);
            this.password = ko.observable(Commerce.Config.demoModePassword);

            this._isPortraitOrientation = ko.observable(false);

            this.selectedOperator.subscribe((newValue: Model.Entities.Employee) => {
                if (!ObjectExtensions.isNullOrUndefined(newValue)) {
                    this.operatorId(newValue.StaffId);
                }
            }, this);

            this._showList = ko.observable(false);
            this.applicationVersion = ko.observable("Version: " + Commerce.ViewModelAdapter.getApplicationVersion());

            this.addControl(this._passwordExpiryDialog = new Controls.MessageDialog());

            this.isOperatorIdTextVisible = ko.computed(() => {
                return !this._showList();
            }, this);

            this.isOperatorIdSelectVisible = ko.computed(() => {
                return !this.isOperatorIdTextVisible();
            }, this);

            this.disableInput = ko.computed(() => {
                return this.indeterminateWaitVisible();
            }, this);

            this.disableSignInButton = ko.computed(() => {
                var requiredInputMissing: boolean =
                    StringExtensions.isEmptyOrWhitespace(this.operatorId()) || StringExtensions.isEmptyOrWhitespace(this.password());

                return this.disableInput() || requiredInputMissing;
            }, this);

            var deviceConfiguration: Model.Entities.DeviceConfiguration
                = ApplicationContext.Instance.deviceConfiguration;

            if (deviceConfiguration) {
                if (!Commerce.ObjectExtensions.isNullOrUndefined(deviceConfiguration.Theme)) {
                    Commerce.CSSHelpers.applyThemeAsync(deviceConfiguration);
                }

                this.indeterminateWaitVisible(true);

                if (deviceConfiguration.ShowStaffListAtLogOn) {

                    this.operatorList(JSON.parse(ApplicationStorage.getItem(ApplicationStorageIDs.EMPLOYEE_LIST_KEY)));

                    if (ArrayExtensions.hasElements(this.operatorList())) {
                        this._showList(true);
                    }
                }

                this.indeterminateWaitVisible(false);
            }

            // addOrientationChangedHandler is not yet available for this view. Orientation change event must be tracked manually.  
            // Detect orientation to handle different background images
            this._orientationMediaQuery = matchMedia("(orientation: landscape)");
            this._isPortraitOrientation(!this._orientationMediaQuery.matches);

            var self: LoginViewController = this; // matchMedia does not trigger handler in correct context.
            this._orientationChangedHandler = () => {
                self._isPortraitOrientation(!self._orientationMediaQuery.matches);
            };

            // Add orientation change listener
            this._orientationMediaQuery.addListener(this._orientationChangedHandler);

            this.commonHeaderData = new Controls.CommonHeaderData();
            this.commonHeaderData.viewSectionInfo(false);
            this.commonHeaderData.viewCommonHeader(false);
            this.commonHeaderData.viewCategoryName(false);
            this.commonHeaderData.viewNavigationBar(false);
            this.commonHeaderData.navigationBarVisible(false);
        }

        /**
         * Dispose local bindings.
         */
        public dispose(): void {
            this._orientationMediaQuery.removeListener(this._orientationChangedHandler);
            super.dispose();
        }

        /**
         * Called when view is created.
         * @param {HTMLElement} element The page element.
         */
        public onCreated(element: HTMLElement): void {
            if (ApplicationContext.Instance.deviceConfiguration &&
                ApplicationContext.Instance.deviceConfiguration.CultureName) {
                Commerce.ViewModelAdapter.setApplicationLanguageAsync(ApplicationContext.Instance.deviceConfiguration.CultureName)
                    .fail((errors: Model.Entities.Error[]) => {
                        NotificationHandler.displayClientErrors(errors);
                    });
            }
        }

        /**
         * Called when view is shown.
         */
        public onShown(): void {
            // enable extended logon with barcode
            Commerce.Peripherals.instance.barcodeScanner.enableAsync(
                (barcode: string): void => {
                    this.extendedLogon(barcode, Authentication.Providers.CommerceUserAuthenticationProvider.EXTENDEDLOGON_BARCODE_GRANT_TYPE);
                });

            // enable extended logon with MSR
            Commerce.Peripherals.instance.magneticStripeReader.enableAsync(
                (cardInfo: Model.Entities.CardInfo): void => {
                    this.extendedLogon(cardInfo.CardNumber, Authentication.Providers.CommerceUserAuthenticationProvider.EXTENDEDLOGON_MSR_GRANT_TYPE);
                });

            if (!Config.isDemoMode) {
                // clear sensitive information
                this.operatorId(StringExtensions.EMPTY);
                this.password(StringExtensions.EMPTY);
            }

            this.viewModel.initializeOfflineDataSync();
            $("#operatorTextBox").focus();
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
         * The handler bound to html actions.
         */
        public signInHandler(): void {
            this.signInAsync(this.password());
        }

        /**
         * Logon Error handler.
         * @param {Model.Entities.Error[]} error The error.
         * @return {IVoidAsyncResult} The async result.
         */
        private handleErrorAsync(errors: Model.Entities.Error[], extendedLogon: string, grantType: string): IVoidAsyncResult {

            var result: VoidAsyncResult = new VoidAsyncResult();

            // Check if change password error
            if (ErrorHelper.hasError(errors, ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_USERPASSWORDEXPIRED.serverErrorCode)) {
                // User must change password
                var dialogDisplayString: string =
                    Commerce.ViewModelAdapter.getResourceString(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_USERPASSWORDEXPIRED.messageResource);
                var displayMessageResult: IVoidAsyncResult = Commerce.ViewModelAdapter.displayMessage(dialogDisplayString,
                    Commerce.MessageType.Info,
                    Commerce.MessageBoxButtons.Default)
                    .always(() => {
                        var options: IChangePasswordViewOptions = {
                            staffId: this.operatorId()
                        };
                        Commerce.ViewModelAdapter.navigate("ChangePasswordView", options);
                    });
                result.resolveOrRejectOn(displayMessageResult);
            } else if (this.viewModel.tokenValidationErrorExists(errors)) {
                ApplicationStorage.clear();
                Utilities.OfflineHelper.stopOffline();
                result.resolve();
                Commerce.ViewModelAdapter.navigate(Helpers.DeviceActivationHelper.GUIDED_ACTIVATION_VIEW_NAME);
            } else if (LoginViewHelper.isPasswordRequired(errors)) {
                LoginViewHelper.handlePasswordRequiredDialog()
                    .on(DialogResult.OK, (password: string) => {
                        this.signInAsync(password, extendedLogon, grantType, /*forcible:*/ true);
                    })
                    .onAny(() => {
                        result.resolve();
                    });
            } else {
                result.resolve();
            }

            return result;
        }

        /**
         * Try to sign In.
         */
        private signInAsync(password: string, extendedLogon?: string, grantType?: string, forcible?: boolean): IVoidAsyncResult {

            if (this._isSigningIn && !forcible) {
                return;
            }

            this._isSigningIn = true;

            var firstTimeUseStr: string = ApplicationStorage.getItem(ApplicationStorageIDs.FIRST_TIME_USE);
            if ("true" === firstTimeUseStr) {
                this.firstTimeUsageVisible(true);
            } else {
                this.indeterminateWaitVisible(true);
            }

            RetailLogger.viewsLoginLoginViewSignInStarted();

            var asyncQueue: AsyncQueue = new AsyncQueue();

            asyncQueue.enqueue(() => {
                var result: VoidAsyncResult = new VoidAsyncResult();
                this.viewModel.operatorLogOn(this.operatorId(), password, extendedLogon, grantType)
                    .done(() => {
                        UserActivityTracker.setUpserverConfiguredAutoLogOffTimeout();

                        // Open dual display with an empty cart
                        if (Peripherals.instance && Commerce.Peripherals.instance.dualDisplay) {
                            Peripherals.instance.dualDisplay.initialize(ApplicationContext.Instance.deviceConfiguration);
                        }
                        result.resolve();
                    })
                    .fail((errors: Model.Entities.Error[]) => {
                        this.handleErrorAsync(errors, extendedLogon, grantType).always(() => {
                            result.reject(errors);
                        });
                    });
                return result;
            }).enqueue(() => {
                var asyncResult: VoidAsyncResult = new VoidAsyncResult(this);

                // Display password expiry dialog if needed - otherwise continue to the next step
                if (Commerce.Session.instance.CurrentEmployee.NumberOfDaysToPasswordExpiry > 0) {
                    var formatString: string = Commerce.Session.instance.CurrentEmployee.NumberOfDaysToPasswordExpiry === 1 ? "string_519" : "string_515";

                    var passwordExpiryDialogOptions: Commerce.Controls.MessageDialogState = {
                        title: Commerce.ViewModelAdapter.getResourceString("string_518"),
                        content: StringExtensions.format(Commerce.ViewModelAdapter.getResourceString(formatString),
                            Commerce.Session.instance.CurrentEmployee.NumberOfDaysToPasswordExpiry),
                        buttons: [
                            {
                                label: Commerce.ViewModelAdapter.getResourceString("string_516"),
                                operationId: Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK,
                                isPrimary: true
                            },
                            {
                                label: Commerce.ViewModelAdapter.getResourceString("string_517"),
                                operationId: Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK,
                                isPrimary: false
                            }
                        ]
                    };

                    var dialogResult: IAsyncDialogResult<DialogResult> =
                        this._passwordExpiryDialog.show(passwordExpiryDialogOptions, true);
                        dialogResult.on(DialogResult.Yes, () => {
                        // Navigate to change password view and stop login process
                        var options: IChangePasswordViewOptions = {
                            staffId: Commerce.Session.instance.CurrentEmployee.StaffId
                        };
                        Commerce.ViewModelAdapter.navigate("ChangePasswordView", options);

                        asyncResult.reject(null);
                    });
                    dialogResult.on(DialogResult.No, () => {
                        // Continue with login process
                        asyncResult.resolve();
                    });
                } else {
                    asyncResult.resolve();
                }

                return asyncResult;
            }).enqueue(() => {
                RetailLogger.viewsLoginLoginViewSignInFinished();
                this.firstTimeUsageVisible(false);

                return this.shiftViewModel.openOrResumeShift(this.operatorId())
                    .done((result: ICancelableResult) => {
                        if (result.canceled) {
                            this.logOffAndDisplayErrors(null);
                        } else {
                            this.navigateToNextPage();
                        }
                    })
                    .fail((errors: Proxy.Entities.Error[]) => {
                        this.logOffAndDisplayErrors(errors);
                    })
                    .always(() => {
                        this.indeterminateWaitVisible(false);
                    });
            });

            return asyncQueue.run()
                .done(() => {
                    ApplicationStorage.setItem(ApplicationStorageIDs.FIRST_TIME_USE, StringExtensions.EMPTY);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    this.firstTimeUsageVisible(false);
                })
                .always(() => {
                    this._isSigningIn = false;
                });
        }

        
        private disableNavigation(eventObject: Event): void {
            eventObject.preventDefault();
            eventObject.cancelBubble = true;
        }
        

        /*
         * Navigate to the home page.
         */
        private navigateToNextPage(): void {
            Commerce.ViewModelAdapter.navigate(Commerce.ApplicationContext.Instance.tillLayoutProxy.startView); // Use AX default start screen for this
        }

        private logOffAndDisplayErrors(errors: Model.Entities.Error[]): void {
            this.viewModel.logOff(this)
                .done(() => {
                    if (errors) {
                        NotificationHandler.displayClientErrors(errors);
                    }
                }).fail((defaultErrors: Proxy.Entities.Error[]) => {
                    if (errors) {
                        NotificationHandler.displayClientErrors(errors);
                    } else {
                        NotificationHandler.displayClientErrors(defaultErrors,
                            Commerce.ViewModelAdapter.getResourceString("string_509")); // Logoff error
                    }
                }).always(() => {
                    this.indeterminateWaitVisible(false);
                });
        }

        private extendedLogon(extendedCredentials: string, grantType: string, password?: string): void {
            if (Session.instance.isLoggedOn) {
                return;
            }
            this.operatorId(null);
            this.password(null);
            this.signInAsync(password, extendedCredentials, grantType);
        }
    }
}