/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */




///<reference path='../Commerce.Core.d.ts'/>

module Commerce {
    "use strict";

    
     export var ViewModelAdapterWinJS: ViewModelAdapterImpl;
    

    /**
     * Method signature for the pageLoadCallBack function.
     */
     export declare type PageLoadCallBackFunction = (data: any[]) => IAsyncResult<any[]>;

     export class ViewModelAdapterImpl implements IViewModelAdapter {
        /*  Language/culture prefixes for regional cultures such as "ar-SA". */
        private static RIGHTTOLEFT_LANGUAGES: string[] = ["ar", "dv", "fa", "he", "prs", "ps", "syr", "ug", "ur"];
        private static RIGHTTOLEFT_REGIONALLANGUAGES: string[] = ["ku-Arab", "pa-Arab", "sd-Arab", "qps-plocm"];
        private static HOME_VIEW: string = "HomeView";

        LOGIN_VIEW: string = "LoginView";
        destinationPagePath: string = StringExtensions.EMPTY;
        messageQueue: Array<any> = [];
        navigateActions: any = {};

       /*region ICore */

       /**
        * Navigate to a page.
        * @param {string} page path.
        * @param {any} optional parameters to pass to a page.
        */
        public navigate(action: string, initialState?: any) {
            this.destinationPagePath = this.navigateActions[action];
            Commerce.Navigator.navigate(action, this.destinationPagePath, initialState);
        }

       /**
        * Navigates backwards in the pagestack by 1 step.
        * @param {any} the caller context.
        * @param {function()} the callback function.
        */
        public navigateBack(callerContext?: any, callBack?: () => void) {
            Navigator.navigateBack();
            if (!ObjectExtensions.isNullOrUndefined(callerContext) && !ObjectExtensions.isNullOrUndefined(callBack)) {
                callBack.call(callerContext);
            }
        }

       /**
        * Bind template to an object.
        * @param {string} element containing template.
        * @param {any} object to bind.
        */
        public bind(template: Element, source: any) {
            WinJS.Binding.processAll(template, WinJS.Binding.as(source));
            ko.applyBindings(source, template);
        }

       /**
        * Verify if we are at the given page path.
        * @param {string} page path.
        */
        public isInView(pagePath: string): boolean {
            // convert both strings (loc and pagePath) to lowercase to ensure that
            // case doesn't affect the test.
            var loc = Commerce.Navigator.pageControl.uri.toUpperCase();

            return (loc.indexOf(pagePath.toUpperCase()) >= 0);
        }

        /**
         * Navigate to home page.
         */
        public goHome() {
            this.navigate(ViewModelAdapterImpl.HOME_VIEW);
        }

       /**
        * Function for getting the resource string and checking if there is a DB override.
        * @param {string} The resource name.
        * @return {string} The localized string if given a valid string ID. Otherwise, the resource name given.
        */
        public getResourceString(resourceName: string): string {
            // search for any overrides in DB 
            var resourceValueOverride: Model.Entities.LocalizedString = ApplicationContext.Instance.customUIStringsMap.getItem(resourceName);

            var localizedMessage: string;
            if (ObjectExtensions.isNullOrUndefined(resourceValueOverride)) {
                // if no override, call the platform specific way of getting a resource string
                localizedMessage = WinJS.Resources.getString(resourceName).value;
            } else {
                localizedMessage = resourceValueOverride.Text;
            }

            // If no localized string is found and we were given a string ID, then the string ID is invalid and we log an error.
            // Otherwise, if no localized string is found and we weren't given a string ID, we return the given string without logging an error.
            if (!StringExtensions.isNullOrWhitespace(resourceName) && resourceName === localizedMessage && StringExtensions.beginsWith(resourceName, "string_", true)) {
                RetailLogger.coreCannotMapResourceMessage(resourceName);
            }

            return localizedMessage;
        }

       /**
        * Sets the application language to the new value specified.
        * @param {string} the new language tag.
        * @return {IVoidAsyncResult} The async result.
        */
        public setApplicationLanguageAsync(languageTag: string): IVoidAsyncResult {
            languageTag = languageTag || this.getDefaultUILanguage();
            return Commerce.Host.instance.globalization.setApplicationLanguageAsync(languageTag).done(() => {
                Model.Managers.Factory.updateContextLocale(languageTag);
                this.changeUILayoutDirection(languageTag);
            });
        }


        /**
         * Retrieve the current language of the application.
         * @return {string} the application language.
         */
        public getCurrentAppLanguage(): string {
            return Commerce.Host.instance.globalization.getApplicationLanguage() || "en-US";
        }

       /**
        * Get the default UI language.
        * @return {string} The resource string
        */
        public getDefaultUILanguage(): string {
            /* This is the default language from manifest and "en-US" will be the last resort. */
            return Commerce.Host.instance.globalization.getDefaultLanguageTag() || "en-US";
        }

        /**
         * Navigate to login page.
         * If device is activated navigate to login page else navigate to device activation page.
         */

        public navigateToLoginPage(isAppInitializing?: boolean) {
            if (ApplicationStorage.getItem(ApplicationStorageIDs.INITIAL_SYNC_COMPLETED_KEY) === "true") {
                // in some cases, app is closed before logoff is performed
                // so clear cache locally (does not navigate or call other services) to remove any stale tokens kept locally
                if (Config.aadEnabled) {
                    Host.instance.azureActiveDirectoryAdapter.clearCache();
                }

                this.navigate(this.LOGIN_VIEW);
            } else if (ApplicationStorage.getItem(ApplicationStorageIDs.AAD_LOGON_IN_PROCESS_KEY) === "true") {
                ApplicationStorage.setItem(ApplicationStorageIDs.AAD_LOGON_IN_PROCESS_KEY, StringExtensions.EMPTY);
                var activationParameters: string = ApplicationStorage.getItem(ApplicationStorageIDs.ACTIVATION_PAGE_PARAMETERS_KEY);
                Helpers.DeviceActivationHelper.navigateToActivationPage(activationParameters);
            } else if (isAppInitializing) {
                ViewModelAdapter.navigate(Helpers.DeviceActivationHelper.DEVICE_ACTIVATION_GET_STARTED_VIEW_NAME);
            } else {
                Helpers.DeviceActivationHelper.navigateToActivationPage();
            }
        }

       /**
        * Get the required month name given its index.
        * @param {string} monthIndex The month's zero-based index.
        */
        public getMonthName(monthIndex: number): string {
            var dateformatter = Commerce.Host.instance.globalization.getDateTimeFormatter(Commerce.Host.Globalization.DateTimeFormat.MONTH_FULL);
            var dateToFormat = new Date();
            dateToFormat.setMonth(monthIndex);

            return dateformatter.format(dateToFormat);
        }

        /**
         * Display a message to the client with the provided display/message options
         * @param {string} message to be shown to the user.
         * @param {IMessageOptions} [messageOptions] The message display options.
         * @return {IAsyncResult<IMessageResult>} The async result.
         */
        public displayMessageWithOptions(message: string, messageOptions: Commerce.IMessageOptions): IAsyncResult<IMessageResult> {
            // Check the parameters
            if (ObjectExtensions.isNullOrUndefined(messageOptions)) {
                messageOptions = new MessageOptions();
            }

            var messageType: string = !ObjectExtensions.isNullOrUndefined(messageOptions.messageType)
                ? MessageType[messageOptions.messageType] : null;
            RetailLogger.userMessageDisplay(messageType, messageOptions.title, message);

            // Get the message dialog
            var dlg = new Commerce.Controls.MessageDialog();

            // This fixes issue with concurrent usage of MessageDialog object. WinRT cannot show more than one MessageDialog at a time.
            // If we will try to show more than one MessageDialog an "Access Denied" exception will be thrown and application will shut down.
            // Here, the idea is to create a message dialogs queue and show them one by one.
            var asyncResult = new AsyncResult<Commerce.IMessageResult>(null);

            // Get the buttons to show in the dialog
            var buttons: any[] = this.getMessageDialogButtons(messageOptions.messageButtons, messageOptions.primaryButtonIndex);

            // Show the dialog
            var asyncDialogResult = dlg.show(<Controls.CheckboxMessageDialogState>{
                title: messageOptions.title || "",
                content: message,
                additionalInfo: messageOptions.additionalInfo,
                buttons: buttons,
                messageCheckboxVisible: messageOptions.displayMessageCheckbox,
                messageCheckboxChecked: messageOptions.messageCheckboxChecked,
                messageCheckboxLabelResourceID: messageOptions.messageCheckboxLabelResourceID
            });

            asyncDialogResult.onAny((result: DialogResult) => {
                asyncResult.resolve({ dialogResult: result, messageCheckboxChecked: dlg.MessageCheckboxChecked });
            });

            return asyncResult;
        }

        /**
         * Display a message to the client
         * @param {string} message to be shown to the user.
         * @param {MessageType} [messageType = MessageType.Info] Type of the message to be shown.
         * @param {MessageBoxButtons} [messageButtons = MessageBoxButtons.Default] Type of the buttons to be shown.
         * @param {string} [title] The message box title.
         * @param {number} [primaryButtonIndex] Allows the primary action to be specified.
         * @return {IAsyncResult<DialogResult>} The async result.
         */
        public displayMessage(message: string, messageType?: MessageType, messageButtons?: MessageBoxButtons, title?: string, primaryButtonIndex?: number): IAsyncResult<DialogResult> {
            // Get the message dialog
            var dlg = new Commerce.Controls.MessageDialog();

            // This fixes issue with concurrent usage of MessageDialog object. WinRT cannot show more than one MessageDialog at a time.
            // If we will try to show more than one MessageDialog an "Access Denied" exception will be thrown and application will shut down.
            // Here, the idea is to create a message dialogs queue and show them one by one.
            var asyncResult = new AsyncResult<DialogResult>(null);
            var buttons: any[] = this.getMessageDialogButtons(messageButtons, primaryButtonIndex);
            var asyncDialogResult = dlg.show({
                title: title || "",
                content: message,
                additionalInfo: null,
                buttons: buttons
            });

            asyncDialogResult.onAny((result: DialogResult) => {
                asyncResult.resolve(result);
            });

            return asyncResult;
        }

       /**
        * Displays a retry message to the client on call failure with retry
        * @param {any} callingContext A handler to the calling context to be used as the viewModel for the callbacks.
        * @param {(errorCallback: (error: Commerce.Model.Entities.Error[]) => void) => void} actionCall The call to retry. On success, the method should handle the callback.
        * @param {(error: Commerce.Model.Entities.Error[]) => void} errorCallback The method to call upon the action call failing.
        * @param {string} defaultMessage Message to be shown to the user if one is not available from the error.
        * @param {string} title option The value for the message title. Otherwise, the title for the error is used.
        */
        public displayRetryMessageAsync(callingContext: any,
            actionCall: (errorCallback: (error: Commerce.Model.Entities.Error[]) => void) => void,
            errorCallback: (error: Commerce.Model.Entities.Error[]) => void,
            defaultMessage: string,
            title?: string): void {

            actionCall((errors: Commerce.Model.Entities.Error[]) => {

                // Not retryable errors, return the errors
                if (!ErrorHelper.isRetryable(errors)) {
                    errorCallback.call(callingContext, errors);
                    return;
                }

                var error: Commerce.Model.Entities.Error = errors[0];

                // Set the message
                var message: string = Commerce.ViewModelAdapter.getResourceString(error.ErrorCode);

                if (ObjectExtensions.isNullOrUndefined(message) || StringExtensions.isEmptyOrWhitespace(message)) {
                    if (ObjectExtensions.isNullOrUndefined(defaultMessage) || StringExtensions.isEmptyOrWhitespace(defaultMessage)) {
                        message = "";
                    } else {
                        message = defaultMessage;
                    }
                }

                // Set the title
                if (ObjectExtensions.isNullOrUndefined(title)) {
                    title = Commerce.ViewModelAdapter.getResourceString(error.ErrorTitleResourceId);
                }

                // Add the question on whether the action should be tried again
                message = message + Commerce.ViewModelAdapter.getResourceString("string_29005");

                // Create the message dialogs
                Commerce.ViewModelAdapter.displayMessage(message, MessageType.Info, MessageBoxButtons.YesNo,
                    (ObjectExtensions.isNullOrUndefined(title)
                    || StringExtensions.isEmptyOrWhitespace(title)) ? null : title)
                    .done((result: DialogResult) => {
                        var nextAction: () => void;
                        if (result === DialogResult.Yes) {
                            nextAction = () => {
                                this.displayRetryMessageAsync(callingContext, actionCall, errorCallback, message, title);
                            };
                        } else {
                            nextAction = () => { errorCallback.call(callingContext, error); };
                        }
                        this.messageQueue.push(nextAction);
                    });

                if (this.messageQueue.length > 0) {
                    var nextAction = this.messageQueue.shift();
                    nextAction();
                }
            });
        }

        /**
         * Gets the application version number.
         * @return {string} The application version number.
         */
        public getApplicationVersion(): string {
            var version = Commerce.Host.instance.application.getApplicationIdentity().version;
            return StringExtensions.format("{0}.{1}.{2}.{3}", version.major, version.minor, version.build, version.revision);
        }

       /**
        * Gets the application publisher name.
        * @return {string} The application publisher name.
        */
        public getApplicationPublisher(): string {
            return Commerce.Host.instance.application.getApplicationIdentity().publisher;
        }

        /**
         * Copies the string message to clipboard.
         * @param {string} message The string to be copied to the clipboard.
         */
        public copyToClipboard(message: string): void {
            var clipboardData: DataTransfer = (<any>window).clipboardData;
            clipboardData.setData("Text", message);
        }

        /*endregion ICore */


        /* Windows specific methods. These are not required in in shared viewmodel. */

        /**
         * Define view. WinJS specific method.
         * @param {string} path The path to html page.
         * @param {string} action The action name.
         * @param {any} [viewControllerType] The view controller type.
         */
        public define(path: string, action: string, viewControllerType?: any) {
            this.navigateActions[action] = path;

            return WinJS.UI.Pages.define(path, <any>{
                // we create the view controller on the init
                init: function (element: HTMLElement, options) {
                    if (viewControllerType) {
                        // we use 'this.', so we can access the viewController on different methods below
                        this.viewController = new viewControllerType(options);
                    }
                },
                // processed is called when the element for the page is created
                processed: function (element: HTMLElement, options) {
                    if (this.viewController && this.viewController.onCreated) {
                        this.viewController.onCreated(element);
                    }
                },
                // ready is called when the element for the page is added to the DOM
                ready: function (element: HTMLElement, options) {
                    if (this.viewController) {
                        if (this.viewController.load) {
                            this.viewController.load();
                        }

                        if (this.viewController.onShown) {
                            this.viewController.onShown();
                        }
                    }

                    Commerce.ViewModelAdapterWinJS.bind(element, this.viewController);

                    if (this.viewController && this.viewController.afterBind) {
                        this.viewController.afterBind();
                    }
                },

                unload: function () {
                    if (this.viewController && this.viewController.unload) {
                        this.viewController.unload();
                    }
                }
            });
        }

        /**
         * Define view. WinJS specific method.
         * @param {string} path path to html page.
         * @param {any} viewControllerType view model type.
         */
        public defineControl(path: string, viewControllerType: any) {
            var userControlType = Commerce.Controls.UserControl;

            if (viewControllerType && Commerce.ObjectExtensions.isOfType(viewControllerType.prototype, userControlType)) {
                viewControllerType.prototype._viewPath = path;
            }

            return WinJS.UI.Pages.define(path, <any>{
                // _viewControllerType is used by userControl knockout binding
                _viewControllerType: viewControllerType,
                _isDefined: true,
                init: function (element: HTMLElement, options) {
                    // if options is a UserControl, use it as the viewController, otherwise, create a new type with options in the constructor
                    if (options && ObjectExtensions.isOfType(options, userControlType)) {
                        this.viewController = options;
                    } else {
                        this.viewController = new viewControllerType(options);
                    }
                },
                // processed is called when the element for the control is created
                processed: function (element: HTMLElement, options) {
                    if (this.viewController.onCreated) {
                        this.viewController.onCreated();
                    }
                },
                // ready is called when the element for the control is added to the DOM
                ready: function (element: HTMLElement, options) {
                    if (this.viewController.onLoaded) {
                        this.viewController.onLoaded();
                    }

                    Commerce.ViewModelAdapterWinJS.bind(element, this.viewController);
                }
            });
        }

        /**
         * Verifies if there is a control defined given the path.
         * @param {string} path The control path.
         * @return {boolean} <c>True</c>, if there is a control defined given the path, or <c>false</c> otherwise.
         */
        public isControlDefined(path: string): boolean {
            return WinJS.UI.Pages.get(path).prototype._isDefined;
        }

        /**
         * Create adapter for incremental list view.
         * @param {any} winControl The control that requires the functionality for pagination.
         * @param {any} callerContext The context in which the callerMethod is invoked.
         * @param {Function} callerMethod The method that is called when more data is needed.
         * @param {number} pageSize Page size for the view.
         * @param {PageLoadCallBackFunction} pageLoadCallBack The method that is called after a new page is loaded.
         */
        public createIncrementalDataSourceAdapter(winControl: any, callerContext: any, callerMethod: Function, pageSize: number, afterLoadComplete: string, onLoading: Observable<boolean>, autoSelectFirstItem: boolean, pageLoadCallBack: PageLoadCallBackFunction) {
            var results = new WinJS.Binding.List([]);

            // Swap builtin template handler with custom one
            var itemTemplate = winControl.itemTemplate;
            var $winControlElement = $(winControl.element);

            var getMoreDataAndAppend = (data: WinJS.Binding.List<any>): IVoidAsyncResult => {
                // Get data.
                var asyncResult: IAsyncResult<any[]> = (<Function>callerMethod).call(callerContext, pageSize, data.length);

                asyncResult.done((pageData) => {
                    var firstIndex: number = data.length;

                    if (ArrayExtensions.hasElements(pageData)) {
                        for (var i = 0; i < pageData.length; i++) {
                            var dataItem = pageData[i];
                            data.push(dataItem);
                        }
                    }

                    var lastIndex: number = data.length;

                    if (ObjectExtensions.isFunction(pageLoadCallBack) && lastIndex > firstIndex) {
                        pageLoadCallBack.call(callerContext, data.slice(firstIndex, lastIndex))
                            .done((updatedData: any[]) => {
                                if (!ObjectExtensions.isNullOrUndefined(updatedData)) {
                                    for (var index: number = firstIndex; index < lastIndex; index++) {
                                        data.setAt(index, updatedData[index - firstIndex]);
                                    }
                                }
                            });
                    }
                });

                return asyncResult;
            };

            // Create template handler that handles template activation for each item
            var overrideTemplate = (template: any, data: WinJS.Binding.List<any>, templateElement?: HTMLElement): any => {
                var fetching = false; // To avoid multiple calls adding lock mechanism.

                return (itemPromise): void => {

                    return itemPromise.then((item) => {
                        // Execute get more data call only when last item in the grid is activated.
                        if (data.length >= pageSize && item.key === data.getItem(data.length - 1).key && !fetching) {
                            fetching = true;

                            // Get data.
                            getMoreDataAndAppend(data).always(() => {
                                fetching = false;
                            });
                        }

                        // Template switch requires change in how items are rendered.
                        if (templateElement) {
                            var container = document.createElement("div");
                            templateElement.winControl.render(item.data, container);

                            return container;
                        }

                        return template(itemPromise);
                    });
                };
            };

            // Template override helper method that is used by template switch
            winControl.setTemplate = (templateElement: HTMLElement) => {
                winControl.itemTemplate = overrideTemplate(itemTemplate, results, templateElement);
            };

            winControl.itemDataSource = results.dataSource;
            winControl.itemTemplate = overrideTemplate(itemTemplate, results);

            var afterLoadCompleteElement = $(afterLoadComplete);
            if (Commerce.ObjectExtensions.isFunction(onLoading)) {
                onLoading(true);
            }
            afterLoadCompleteElement.hide();

            getMoreDataAndAppend(results).always(() => {
                afterLoadCompleteElement.hide();
                if (Commerce.ObjectExtensions.isFunction(onLoading)) {
                    onLoading(false);
                }

                if (results.length < 1) {
                    afterLoadCompleteElement.show();
                    $winControlElement.hide();
                } else {
                    if ($winControlElement.is(":hidden")) {
                        $winControlElement.show();
                    }
                    // select the first item in the list view
                    if (autoSelectFirstItem === true &&
                        winControl.selection.count() === 0) {
                        winControl.selection.set(0);
                    }
                }
            }); // Make first call to get data.
        }

        /**
         * Bind report data parameters.
         * Sets the id of the control to the value of the internalControlId attribute, if defined, set in the control placeholder element.
         * @param {element} control name.
         * @param {data} bound array.
         */
        public bindReportParameters(element: any, data: any) {
            $(element).find(".controlPlaceholder").each(function (index, controlPlaceholderElement) {
                switch (data.Type) {
                    case "DateTime":
                        var datePicker: any = new WinJS.UI.DatePicker(<HTMLElement>controlPlaceholderElement);
                        datePicker.current = data.Value;
                        datePicker.datePattern = "{day.integer(2)} {dayofweek.full}";
                        datePicker.addEventListener("change", function (eventInfo) {
                            data.Value = eventInfo.currentTarget.winControl.current;
                        });

                        // Set the id of the first control in the DatePicker control to the internalControlId of the control placeholder element
                        if (controlPlaceholderElement.hasAttribute("internalControlId")) {
                            var $firstDatePickerControl = $(controlPlaceholderElement).children().first();
                            if ($firstDatePickerControl) {
                                $firstDatePickerControl.attr("id", controlPlaceholderElement.getAttribute("internalControlId"));
                            }
                         }

                        return datePicker;
                    default:
                        var $wrappingInputDiv = $("<div tabindex='-1' class='minWidth27 maxWidth32'></div>");
                        var $input = $("<input type='text'/>");
                        $input.val(data.Value).blur(function (eventInfo) {
                            data.Value = $(eventInfo.currentTarget).val();
                        });
                        $wrappingInputDiv.append($input);
                        $(controlPlaceholderElement).append($wrappingInputDiv);

                        // Set the id of the input label to the internalControlId of the control placeholder element
                        if (controlPlaceholderElement.hasAttribute("internalControlId")) {
                            $input.attr("id", controlPlaceholderElement.getAttribute("internalControlId"));
                        }

                        return $input;
                }
            });

        }

        /**
         * Add event handlers for global events. This approach should ensure event handlers are disposed with the view.
         * @param {Element} element DOM element that will need to handle event.
         * @param {string} eventName Event name.
         * @param {Function} eventHandler Event handler function for the event.
         */
        public addViewEvent(element: Element, eventName: string, eventHandler: Function): void {
            Navigator.addEventHandler(element, eventName, eventHandler);
        }

        /**
         * Removes event handlers for global events. This is in case controls on the view need to attach and detach events multiple times.
         * @param {Element} element DOM element that will need to handle event.
         * @param {string} eventName Event name.
         * @param {Function} eventHandler Event handler function for the event.
         */
        public removeViewEvent(element: Element, eventName: string, eventHandler: Function): void {
            Navigator.removeEventHandler(element, eventName, eventHandler);
        }

        /**
         * Execute event handlers for a specified event. 
         * @param {string} eventName Event name.
         * @param {any[]} argsArray Array of arguments.
         */
        public raiseViewEvent(eventName: string, ...argArray: any[]): void {
            // Events can be raised before navigator is initialized. 
            if (!ObjectExtensions.isNullOrUndefined(Navigator)) {
                var eventHandlers = Navigator.getHandlers(eventName);
                if (eventHandlers.length > 0) {
                    // if handler adds or removes handlers we need to have a copy to avoid unintended calls (infinite loop).
                    var collectionCopy = eventHandlers.slice(0);
                    while (collectionCopy.length > 0) {
                        var eventHandler = collectionCopy.pop();
                        // Caller context is unknown, calling handler with undefined.
                        eventHandler.apply(undefined, argArray);
                    }
                }
            }
        }

        /**
         * Gets the message dialog buttons with the configured parameters
         * @param {MessageBoxButtons} [messageButtons = MessageBoxButtons.Default] Type of the buttons to be shown.
         * @param {number} [primaryButtonIndex] The index of the button to have default enter behavior.
         * @return {any[]} The array of buttons.
         */
        private getMessageDialogButtons(messageButtons?: MessageBoxButtons, primaryButtonIndex?: number): any[] {

            messageButtons = messageButtons || MessageBoxButtons.Default;
            var buttons = [];

            switch (messageButtons) {

                case MessageBoxButtons.OKCancel:
                    buttons[0] = {
                        label: Commerce.ViewModelAdapter.getResourceString("string_75"),
                        operationId: Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK,
                        result: DialogResult.OK
                    };
                    buttons[1] = {
                        label: Commerce.ViewModelAdapter.getResourceString("string_76"),
                        operationId: Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK,
                        result: DialogResult.Cancel,
                        isPrimary: true,
                        cancelCommand: true
                    };
                    break;

                case MessageBoxButtons.YesNo:
                    buttons[0] = {
                        label: Commerce.ViewModelAdapter.getResourceString("string_77"),
                        operationId: Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK,
                        result: DialogResult.Yes
                    };
                    buttons[1] = {
                        label: Commerce.ViewModelAdapter.getResourceString("string_78"),
                        operationId: Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK, result: DialogResult.No,
                        isPrimary: true,
                        cancelCommand: true
                    };
                    break;

                case MessageBoxButtons.RetryNo:
                    buttons[0] = {
                        label: Commerce.ViewModelAdapter.getResourceString("string_81"),
                        operationId: Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK,
                        result: DialogResult.Yes
                    };
                    buttons[1] = {
                        label: Commerce.ViewModelAdapter.getResourceString("string_78"),
                        operationId: Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK,
                        result: DialogResult.No,
                        isPrimary: true,
                        cancelCommand: true
                    };
                    break;

                case MessageBoxButtons.Default:
                    buttons[0] = {
                        label: Commerce.ViewModelAdapter.getResourceString("string_75"),
                        operationId: Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK,
                        result: DialogResult.Close
                    };

            }

            if (!ObjectExtensions.isNullOrUndefined(primaryButtonIndex)) {
                // doing the loop to insure if a bad value ever passed, no runtime
                // errors and users can proceed without impact.
                for (var i = 0; i < buttons.length; i++) {
                    if (i === primaryButtonIndex) {
                        buttons[i].isPrimary = true;
                    }
                }
            }

            return buttons;
        }

        /**
         * Change the layout/flow direction as per the language, either to rtl or ltr.
         * @param {string} the language tag.
         */
        private changeUILayoutDirection(languageTag: string): void {

            if (Commerce.CSSHelpers.isCSSDeveloperMode() && CSSHelpers.isDeveloperModeTextDirectionSet()) {
                CSSHelpers.setTextDirection(CSSHelpers.getDeveloperModeTextDirection());
            } else {
                var direction: string = CSSHelpers.LEFT_TO_RIGHT_TEXT_DIRECTION;

                if (!StringExtensions.isNullOrWhitespace(languageTag)) {
                    // Get a match if langugeTag is in the form "ku-Arab"
                    var matchedRegionalLanguage = ArrayExtensions.firstOrUndefined<string>(ViewModelAdapterImpl.RIGHTTOLEFT_REGIONALLANGUAGES, (lang) => {
                        return StringExtensions.compare(lang, languageTag, true) === 0;
                    });

                    var matchedLanguage: string = null;

                    // Get a match if languageTag is in the form "ar"
                    if (ObjectExtensions.isNullOrUndefined(matchedRegionalLanguage)) {
                        var fullLanguage = languageTag.split("-", 1);
                        if (!ObjectExtensions.isNullOrUndefined(fullLanguage) && fullLanguage.length === 1) {
                            var language = fullLanguage[0];
                            matchedLanguage = ArrayExtensions.firstOrUndefined<string>(ViewModelAdapterImpl.RIGHTTOLEFT_LANGUAGES, (lang) => {
                                return StringExtensions.compare(lang, language, true) === 0;
                            });
                        }
                    }

                    // If matched with any of the right to left languages, change the direction to rtl
                    if (!ObjectExtensions.isNullOrUndefined(matchedRegionalLanguage) || !ObjectExtensions.isNullOrUndefined(matchedLanguage)) {
                        direction = CSSHelpers.RIGHT_TO_LEFT_TEXT_DIRECTION;
                    }
                }

                CSSHelpers.setTextDirection(direction);
            }
        }
    }

    // Instantiate value for Core and CoreWinJS.
    
    ViewModelAdapterWinJS = ViewModelAdapter = new ViewModelAdapterImpl();
    
}
