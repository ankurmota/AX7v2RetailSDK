/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class SettingsViewController extends ViewControllerBase {
        public commonHeaderData: Controls.CommonHeaderData;

        public applicationName: Observable<string>;
        public applicationVersion: Observable<string>;
        public applicationPublisher: Observable<string>;
        public appSessionId: Observable<string>;
        public userSessionId: Observable<string>;
        public indeterminateWaitVisible: Observable<boolean>;
        public enableBubbleTour: Computed<boolean>;
        public enableVideoTutorial: Computed<boolean>;
        public isTaskRecorderVisible: Computed<boolean>;

        private  NAME_VALUE_TEMPLATE: string = "{0} {1}";

        /**
         * @constructor
         */
        constructor() {
            super(true);
            var appNameId: string = Commerce.Config.isDemoMode ? Commerce.ViewModelAdapter.getResourceString("string_29950")
                : Commerce.Config.appName;
            this.applicationName = ko.observable(appNameId);
            this.applicationVersion = ko.observable(StringExtensions.format(this.NAME_VALUE_TEMPLATE,
                Commerce.ViewModelAdapter.getResourceString("string_2"),
                Commerce.ViewModelAdapter.getApplicationVersion()));
            this.applicationPublisher = ko.observable(StringExtensions.format(this.NAME_VALUE_TEMPLATE,
                Commerce.ViewModelAdapter.getResourceString("string_9"),
                Commerce.ViewModelAdapter.getApplicationPublisher()));
            this.appSessionId = ko.observable(TsLogging.LoggerBase.getAppSessionId());
            this.userSessionId = ko.observable(TsLogging.LoggerBase.getUserSessionId());

            this.enableBubbleTour = ko.computed(() => {
                return !UI.Tutorial.bubbleTourDisabled();
            });

            this.enableVideoTutorial = ko.computed(() => {
                return !UI.Tutorial.tutorialVideoDisabled();
            });

            this.isTaskRecorderVisible = ko.computed(() => { return this.isCreateRecordingAllowed(); });

            // initialize header
            this.commonHeaderData = new Controls.CommonHeaderData();
            this.commonHeaderData.viewSectionInfo(false);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_7400"));
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_7401"));
            this.commonHeaderData.settingsButtonVisible(false);
        }

        /**
         * Reset application.
         */
        public resetApplicationHandler(): void {
            if (Commerce.Session.instance.connectionStatus !== ConnectionStatusType.Online) {
                NotificationHandler.displayErrorMessage(Commerce.ViewModelAdapter.getResourceString("string_7406"));
            } else {
                ApplicationStorage.clear();
                Session.instance.resetSession();
                ApplicationContext.Instance = null;
                Config.retailServerUrl = StringExtensions.EMPTY;
                UI.Tutorial.init();
                Utilities.OfflineHelper.stopOffline();
                ViewModelAdapter.navigate(Helpers.DeviceActivationHelper.DEVICE_ACTIVATION_GET_STARTED_VIEW_NAME);
            }
        }

        /**
         * Toggle to the video tutorial.
         */
        public toggleVideoTutorial(): void {
            UI.Tutorial.tutorialVideoDisabled(!UI.Tutorial.tutorialVideoDisabled());
        }

        /**
         * Toggle to the bubble tour.
         */
        public toggleBubbleTour(): void {
            UI.Tutorial.bubbleTourDisabled(!UI.Tutorial.bubbleTourDisabled());
        }

        /**
         * Activate Task Recorder UI.
         */
        public activateTaskRecorder(): void {
            ViewModelAdapter.navigateBack();
            try {
                Commerce.TaskRecorder.activateTaskRecorder();
            } catch (error) {
                NotificationHandler.displayClientErrors(error);
            }
        }

        /**
         * Returns true, if create recording is allowed on current terminal.
         */
        private isCreateRecordingAllowed(): boolean {
            var deviceConfiguration: Model.Entities.DeviceConfiguration = ApplicationContext.Instance.deviceConfiguration;

            return deviceConfiguration.CreateRecording;
        }
    }
}