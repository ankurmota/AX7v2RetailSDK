/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ApplicationContext.ts'/>
///<reference path='ErrorHandler.ts'/>

module Commerce {
    "use strict";

    /**
     * Class for tracking user session timeout.
     */
    export class UserActivityTracker {
        private static DEFAULT_TIMEOUT: number = 30;
        private static MILLISECONDS_IN_A_MINUTE: number = 60 * 1000;

        private static _userActivityTimeout: number;
        private static _userActivityIntervalId: number;
        private static _lastUserActivityTime: Date;
        private static _userEvents: string = "keypress mousedown touchstart click keydown";

        /**
         * Sets up user client activity timer.
         */
        public static setUpUserActivity(): void {
            if (UserActivityTracker.DEFAULT_TIMEOUT === 0) {
                return;
            }

            UserActivityTracker._userActivityTimeout = UserActivityTracker.DEFAULT_TIMEOUT * UserActivityTracker.MILLISECONDS_IN_A_MINUTE;
            UserActivityTracker.detachHandler();
            $(document).bind(UserActivityTracker._userEvents, UserActivityTracker.userActivityHandler);
            UserActivityTracker.userActivityHandler();
        }

        /**
         * Detaches user activity tracker.
         */
        public static detachHandler(): void {
            if ((UserActivityTracker.DEFAULT_TIMEOUT === 0) || ObjectExtensions.isNullOrUndefined(UserActivityTracker.DEFAULT_TIMEOUT)) {
                return;
            }

            clearInterval(UserActivityTracker._userActivityIntervalId);
            $(document).unbind(UserActivityTracker._userEvents, UserActivityTracker.userActivityHandler);
        }

        public static setUpserverConfiguredAutoLogOffTimeout(): void {
            var autoLogOffTimeOutConfig: number = ApplicationContext.Instance.deviceConfiguration.AutoLogOffTimeoutInMinutes;
            if (ObjectExtensions.isNullOrUndefined(autoLogOffTimeOutConfig)) {
                UserActivityTracker._userActivityTimeout = UserActivityTracker.DEFAULT_TIMEOUT * UserActivityTracker.MILLISECONDS_IN_A_MINUTE;
                UserActivityTracker.userActivityHandler();
            } else if (autoLogOffTimeOutConfig <= 0) {
                UserActivityTracker.detachHandler();
            } else {
                UserActivityTracker._userActivityTimeout = autoLogOffTimeOutConfig * UserActivityTracker.MILLISECONDS_IN_A_MINUTE;
                UserActivityTracker.userActivityHandler();
            }
        }

        /**
         * Handles user client activities.
         */
        private static userActivityHandler(): void {
            UserActivityTracker._lastUserActivityTime = new Date();

            clearInterval(UserActivityTracker._userActivityIntervalId);

            UserActivityTracker._userActivityIntervalId = setInterval((): void => {
                if (new Date() > UserActivityTracker._lastUserActivityTime) {
                    ErrorHandler.handleUserAuthenticationException();
                }
            }, UserActivityTracker._userActivityTimeout);
        }
    }
}