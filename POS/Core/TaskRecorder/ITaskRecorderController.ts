/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="TaskRecorderSession.ts" />
/// <reference path="TaskRecorderManager.ts" />

module Commerce.TaskRecorder {

    export interface ITaskRecorderController {

        /**
         * Navigates to the given page in main panel with preservation of existing page to navigate back.
         * @param {string} pageName The name of a page to navigate.
         * @param {any} options The optional state to be passed to the navigated page.
         */
        navigate(pageName: string, options?: any): void;

        /**
         * Activates the Task Recorder main panel with page related to actual state.
         * @param {string} [viewName] The specific view.
         */
        activateMainPanel(viewName?: string): void;

        /**
         * Deactivates the Task Recorder main panel.
         */
        deactivateMainPanel(): void;

        /**
         * Showes the Task Recorder main panel.
         */
        showMainPanel(): void;

        /**
         * Hides the Task Recorder main panel.
         */
        hideMainPanel(): void;

        /**
         * Toggles the Task Recorder main panel.
         */
        toggleMainPanel(): void;
    }
}
