/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

declare module Commerce {
    var ViewModelAdapter: IViewModelAdapter;

    interface IViewModelAdapter {
        LOGIN_VIEW: string;
        destinationPagePath: string;

        /**
        * Navigate to a page.
        *
        * @param {string} page path.
        * @param {any} optional parameters to pass to a page.
        */
        navigate(pagePath: string, initialState?: any): void;

        /**
         * Navigates backwards in the pagestack by 1 step.
         *
         * @param {any} the caller context.
         * @param {function()} the callback function.
         */
        navigateBack(callerContext?: any, callBack?: () => void): void;

        /*
         * Bind template to an object.
         *
         * @param {string} element containing template.
         * @param {any} object to bind.
         */
        bind(template: Element, source: any): void;

        /**
         * Verify if we are at the given page path.
         *
         * @param {string} page path.
         * @return {boolean} if we are at the given page path
         */
        isInView(pagePath: string): boolean;

        /**
         * Navigate to home page.
         */
        goHome(): void;

        /**
         * get the value for the specified resource string
         *
         * @param {string} resource name.
         * @return {string} The resource string
         */
        getResourceString(resourceName: string): string;

        /**
         * Get the default UI language.
         *
         * @return {string} The resource string
         */
        getDefaultUILanguage(): string;

        /**
         * Retrieve the current language of the application.
         * @return {string} the application language.
         */
        getCurrentAppLanguage(): string;

        /**
         * Sets the application language to the new value specified.
         * @param {string} languageTag The new language tag.
         * @return {IVoidAsyncResult} The async result.
         */
        setApplicationLanguageAsync(languageTag: string): IVoidAsyncResult;

        /**
         * Navigate to login page.
         * @param {boolean} True if app is initialzing, false otherwise.
         */
        navigateToLoginPage(isAppInitializing?: boolean): void;

        /**
         * get the required month name, index 0 based
         *
         * @param {number} the month index
         * @return {string} the month name
         */
        getMonthName(monthIndex: number): string;

        /**
         * Display a message to the client
         *
         * @param {string} message to be shown to the user.
         * @param {MessageType} [messageType = MessageType.Info] Type of the message to be shown.
         * @param {MessageBoxButtons} [messageButtons = MessageBoxButtons.Default] Type of the buttons to be shown.
         * @param {string} [title] The message box title.
         * @param {number} [primaryButtonIndex] Allows the primary action to be specified.
         * @return {IAsyncResult<DialogResult>} The async result.
         */
        displayMessage(message: string, messageType?: MessageType, messageButtons?: MessageBoxButtons, title?: string, primaryButtonIndex?: number)
            : IAsyncResult<DialogResult>;

        /**
         * Display a message to the client with the provided display/message options
         *
         * @param {string} message to be shown to the user.
         * @param {IMessageOptions} [messageOptions] The message display options.
         * @return {IAsyncResult<IMessageResult>} The async result.
         */
        displayMessageWithOptions(message: string, messageOptions: Commerce.IMessageOptions)
            : IAsyncResult<IMessageResult>;

        /**
         * Displays a retry message to the client on call failure with retry
         *
         * @param {any} callingContext A handler to the calling context to be used as the viewModel for the callbacks.
         * @param {(errorCallback: (error: Commerce.Model.Entities.Error[]) => void) => void} actionCall The call to retry. On success, the method should handle the callback.
         * @param {(error: Commerce.Model.Entities.Error[]) => void} errorCallback The method to call upon the action call failing.
         * @param {string} defaultMessage Message to be shown to the user if one is not available from the error.
         * @param {string} title option The value for the message title.
         */
        displayRetryMessageAsync(callingContext: any,
            actionCall: (errorCallback: (error: Commerce.Model.Entities.Error[]) => void) => void,
            errorCallback: (error: Commerce.Model.Entities.Error[]) => void,
            defaultMessage: string,
            title?: string);

        /**
         * Bind report data parameters.
         */
        bindReportParameters(element: any, data: any): void;

        /**
         * Returns Version Number.
         *
         * @return {string} The application version.
         */
        getApplicationVersion(): string;

        /**
         * Returns Publisher Name.
         *
         * @return {string} The application publisher name
         */
        getApplicationPublisher(): string;

        /**
         * Copies the string message to clipboard.
         * 
         * @param {string} message The string to be copied to the clipboard.
         */
        copyToClipboard(message: string): void;

        /**
         * Add event handlers for global events. This approach should ensure event handlers are disposed with the view.
         *
         * @param {Element} element DOM element that will need to handle event.
         * @param {string} eventName Event name.
         * @param {Function} eventHandler Event handler function for the event.
         */
        addViewEvent(element: Element, eventName: string, eventHandler: Function): void;

        /**
         * Removes event handlers for global events. This is in case controls on the view need to attach and detach events multiple times.
         *
         * @param {Element} element DOM element that will need to handle event.
         * @param {string} eventName Event name.
         * @param {Function} eventHandler Event handler function for the event.
         */
        removeViewEvent(element: Element, eventName: string, eventHandler: Function): void;

        /**
         * Execute event handlers for a specified event. 
         *
         * @param {string} eventName Event name.
         * @param {any[]} argsArray Array of arguments.
         */
        raiseViewEvent(eventName: string, ...argArray: any[]): void;
    }

    module Config {
        export var connectionTimeout: number;
        export var demoModeDeviceId: string;
        export var demoModeTerminalId: string;
        export var demoModeStaffId: string;
        export var demoModePassword: string;
        export var isDemoMode: boolean;
        export var isDebugMode: boolean;
        export var onlineDatabase: string;
        export var offlineDatabase: string;
        export var retailServerUrl: string;
        export var aadEnabled: boolean;
        export var appHardwareId: string;
        export var locatorServiceEnabled: boolean;
        export var aadLoginUrl: string;
        export var aadClientId: string;
        export var aadRetailServerResourceId: string;
        export var locatorServiceUrl: string;
        export var defaultOfflineDownloadIntervalInMilliseconds: number;
        export var defaultOfflineUploadIntervalInMilliseconds: number;
        export var defaultPageSize: number;
        export var commerceAuthenticationAudience: string;
        export var persistentRetailServerUrl: string;
        export var persistentRetailServerEnabled: boolean;
        export var sqlCommandTimeout: number;
    }
}
