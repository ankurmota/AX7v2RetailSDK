/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.UI {
    "use strict";

    export class Tutorial {

        /**
         * Indicates that video is disabled.
         */
        public static tutorialVideoDisabled: Observable<boolean>;

        /**
         * Indicates that tour is disabled.
         */
        public static bubbleTourDisabled: Observable<boolean>;

        /**
         * Indicates the bubble tour is in progress.
         */
        public static isBubbleTourInProgress: Observable<boolean>;

        private static activeLocation: string;
        private static isTutorialStarted: boolean;
        private static tutorialVideoDialog: Controls.TutorialVideoDialog;
        private static startPage: string = "HomeView";
        private static bubbleControl: UI.HelpBubbles.BubbleControl;
        private static pageElement: HTMLElement;

        private static arrowColor: string = "#0F6FCF";
        private static accentColors: { [key: number]: string } = {
            13004559: "#8D4294", // Blue
            3776084: "#0F6FCF", // Green
            2376177: "#8D4294", // Red
            13850523: "#0F6FCF" // Purple
        };

        /**
         * Initialize tutorial.
         */
        public static init(): void {
            Tutorial.bubbleControl = UI.HelpBubbles.BubbleControl.instance();
            Tutorial.isBubbleTourInProgress = Tutorial.bubbleControl.isInProgress;
            Tutorial.isTutorialStarted = false;
            Tutorial.tutorialVideoDisabled = ko.observable(false);
            Tutorial.bubbleTourDisabled = ko.observable(false);
            Tutorial.bubbleTourDisabled.subscribe((value: boolean) => {
                ApplicationStorage.setItem(ApplicationStorageIDs.BUBBLE_TOUR_DISABLED, JSON.stringify(value));
            });
            Tutorial.tutorialVideoDisabled.subscribe((value: boolean) => {
                ApplicationStorage.setItem(ApplicationStorageIDs.VIDEO_TUTORIAL_DISABLED, JSON.stringify(value));
            });
            Tutorial.tutorialVideoDialog = new Controls.TutorialVideoDialog();
        }

        /**
         * Happens before the navigation happens to hide tutorial.
         */
        public static onBeforeNavigate(): void {
            if (!Tutorial.isTutorialModeEnabled()) {
                return;
            }

            if (Tutorial.isBubbleTourInProgress()) {
                Tutorial.bubbleControl.onBeforeNavigate();
            }
        }

        /**
         * Should be called when navigation happens.
         */
        public static onAfterNavigate(location: string, element: HTMLElement): void {
            if (!Tutorial.isTutorialModeEnabled()) {
                Tutorial.isTutorialStarted = false;
                if (Tutorial.isBubbleTourInProgress()) {
                    Tutorial.bubbleControl.stopTour(false);
                }

                return;
            }

            if (ObjectExtensions.isNullOrUndefined(location)) {
                return;
            }

            Tutorial.activeLocation = location;
            Tutorial.pageElement = element;
            if (location === Tutorial.startPage && !Tutorial.isTutorialStarted) {
                Tutorial.startTutorialMode();
            } else if (Tutorial.isBubbleTourInProgress()) {
                Tutorial.bubbleControl.onAfterNavigate(location, element);
            }
        }

        /**
         * Stop the bubble tour.
         */
        public static stopBubbleTour(): void {
            Tutorial.bubbleControl.stopTour(true);
        }

        /**
         * Starts tutorial mode.
         */
        public static startTutorialMode(): void {
            Tutorial.tutorialVideoDisabled(Tutorial.readBooleanSetting(ApplicationStorageIDs.VIDEO_TUTORIAL_DISABLED));
            Tutorial.setColor();
            Tutorial.isTutorialStarted = true;

            if (!Tutorial.tutorialVideoDisabled()) {
                Tutorial.startVideoTutorial().onAny(() => {
                    Tutorial.startBubbleTour();
                });
            } else {
                Tutorial.startBubbleTour();
            }
        }

        /**
         * Start video tutorial.
         */
        public static startVideoTutorial(): IAsyncDialogResult<void> {
            return Tutorial.tutorialVideoDialog.show(null, true);
        }

        /**
         * Start bubble tour.
         */
        public static startBubbleTour(): void {
            Tutorial.bubbleTourDisabled(Tutorial.readBooleanSetting(ApplicationStorageIDs.BUBBLE_TOUR_DISABLED));
            if (Tutorial.isBubbleTourInProgress() || Tutorial.bubbleTourDisabled()) {
                return;
            }

            Tutorial.isBubbleTourInProgress(true);
            Tutorial.bubbleControl.reset(Tutorial.activeLocation, Tutorial.pageElement, Tutorial.arrowColor);
        }

        /**
         * Identifies whether tutorial mode is enabled.
         * @returns {boolean} Indicates whether tutorial mode is enabled.
         */
        public static isTutorialModeEnabled(): boolean {
            return Commerce.Session.instance.isLoggedOn && Commerce.ApplicationContext.Instance.retailTrialPlanOffer;
        }

        /**
         * Toggle the bubble tour.
         */
        public static toggleBubbleTour(): void {
            Tutorial.bubbleTourDisabled(!Tutorial.bubbleTourDisabled());
        }

        private static setColor(): void {
            if (!ObjectExtensions.isNullOrUndefined(CSSHelpers.accentColorId)) {
                Tutorial.arrowColor = Tutorial.accentColors[CSSHelpers.accentColorId];
            }
        }

        private static readBooleanSetting(key: ApplicationStorageIDs): boolean {
            var settingValue: boolean = JSON.parse(ApplicationStorage.getItem(key));
            return ObjectExtensions.isBoolean(settingValue) && settingValue;
        }
    }
}