/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../Controls/dialog/knockout.dialog.ts" />
///<reference path='../Views/IKeepAliveView.ts'/>

module Commerce {
    "use strict";

    export var Navigator: ViewNavigator = null;

    export class ViewNavigator {
        public element = <HTMLElement>null;
        public home = StringExtensions.EMPTY;
        private static _stayAliveViewElementAttribute = 'IKeepAliveView';
        private static _globalEventsAttributeName = "GlobalEvents";
        private static _disposableObjectsTagName = "DisposableObjects";

        // This is the back button history. It should always contain the current page as the top of the stack.
        private _history = [];
        public navigationLog: ObservableArray<string>;
        public stimefmt: Commerce.Host.IDateTimeFormatter;

        // we store only 10 pages in history
        private static NumberOfPagesInHistory: number = 10;

        // Define the constructor function for the ViewNavigator.
        constructor(element: Element, options: { home: string; }) {
            this.stimefmt = Commerce.Host.instance.globalization.getDateTimeFormatter(Commerce.Host.Globalization.DateTimeFormat.LONG_TIME);

            if (!ObjectExtensions.isNullOrUndefined(element)) {
                this.element = <HTMLElement>element;
                $(this.element).empty();
            } else {
                this.element = document.createElement("div");
            }

            var pageElement = this._createPageElement();
            this.element.appendChild(pageElement);
            this._history.push(pageElement);
            this.navigationLog = ko.observableArray<string>([]);

            this.home = options.home;

            window.addEventListener("resize", this._resized.bind(this));
            document.body.addEventListener("keyup", this._keyupHandler.bind(this));
            document.body.addEventListener("keypress", this._keypressHandler.bind(this));
            document.body.addEventListener("mspointerup", this._mspointerupHandler.bind(this));

            Commerce.Navigator = this;
        }

        public get pageControl() { return this.pageElement && this.pageElement.winControl; }
        private get pageElement() { return this._history[this._history.length - 1]; }
        private get backButton(): HTMLButtonElement { return this.pageElement.querySelector("nav .iconNavBack"); }
        private get isBackNavigationEnabled(): boolean { return (this.backButton && !this.backButton.disabled); }

        // This function creates a new container for each page.
        private _createPageElement() {
            var element = <HTMLElement>document.createElement("div");
            element.style.width = "100%";
            element.style.height = "100%";
            return element;
        }

        // This function responds to keypresses to only navigate when
        // the backspace key is not used elsewhere.
        private _keypressHandler(args) {
            if (args.key === "Backspace") {
                this.navigateBack();
            }
        }

        private _keyupHandler(args) {
            if ((args.key === "Left" && args.altKey) || (args.key === "BrowserBack")) {
                this.navigateBack();
            }
        }

        private _mspointerupHandler(args) {
            if (args.button === 3) {
                this.navigateBack();
            }
        }

        private appendNavigationLog(location: string) {
            this.navigationLog().push(this.stimefmt.format(new Date()) + location);
        }

        /** 
         *  Navigates to a new view by adding new pages to the DOM.
         * @param {string} friendly name of the page.
         * @param {string} location Location of view, to be navigated to.
         * @param {any} state. Optional parameter for viewModel instantiation.
         * usage example:
         * Navigator.navigate("/Views/Login/LoginView.html", null);
         */
        public navigate(action: string, location: string, state?: any) {

            RetailLogger.writePageViewEvent(action);
            Commerce.UI.Tutorial.onBeforeNavigate();

            // Clear all content if we navigate to login view.
            if (location === Commerce.Config.viewPaths[Commerce.ViewModelAdapter.LOGIN_VIEW]) {
                this._hideElement(this.pageElement);
                this.clearAllContent();

                // Clear handlers to avoid multi binding of event handlers.
                if (Commerce.ApplicationContext.Instance && Commerce.ApplicationContext.Instance.tillLayoutProxy) {
                    Commerce.ApplicationContext.Instance.tillLayoutProxy.clearOrientationChangedHandlers();
                }
            }

            var oldElement: HTMLElement = <HTMLElement>this.pageElement;

            var keepAliveViewsCollection = $(this.element).find('[' + ViewNavigator._stayAliveViewElementAttribute + '="' + location + '"]');

            this.appendNavigationLog(location);

            // If IKeepAliveView not found create view.
            // Else only use show/hide methods.
            if (keepAliveViewsCollection.length == 0) {
                var newElement = this._createPageElement();
                $(newElement).attr("Action", action);

                var parentedComplete;
                var parented = new WinJS.Promise(function (c) { parentedComplete = c; });

                WinJS.UI.Pages.render(location, newElement, state, parented)
                    .then((control: any) => {
                        this.element.insertAdjacentElement("afterBegin", newElement); // add it as first element in the list.

                        // Add the page we are going to and put it on top of the history.
                        this._checkForAndRemoveNavigationCircle(newElement, oldElement);
                        this._history.push(newElement);

                        // Hide the page we are navigating away from.
                        this._hideElement(oldElement);

                        parentedComplete();

                        //parentedComplete();
                        // Set view attribute on element of IKeepAliveView view.
                        if (control.element.winControl.viewController && control.element.winControl.viewController.keepAliveViewActivated) {
                            $(control.element).attr(ViewNavigator._stayAliveViewElementAttribute, location);
                            control.element.winControl.viewController.keepAliveViewActivated(state);
                        }

                        this._navigated(location, state);
                        UI.Tutorial.onAfterNavigate(action, control.element);
                    });
            }
            else {
                // Swap views and execute IKeepAliveView method
                var keepAliveView: HTMLElement = keepAliveViewsCollection.get(0);
                var winControl: any = keepAliveView["winControl"];

                WinJS.Promise.timeout().then(() => {
                    // Add the page we are going to and put it on top of the history.
                    this._checkForAndRemoveNavigationCircle(keepAliveView, oldElement);

                    this._history.push(keepAliveView);

                    // Hide the page we are navigating away from.
                    this._hideElement(oldElement);

                    this._showElement(keepAliveView);
                    if (winControl
                        && winControl.viewController
                        && winControl.viewController.keepAliveViewActivated) {
                        winControl.viewController.keepAliveViewActivated(state);
                    }

                    this._navigated(location, state);
                    UI.Tutorial.onAfterNavigate(action, keepAliveView);

                });
            }
        }

        private _resized(event: UIEvent) {
            if (this.pageControl && this.pageControl.updateLayout) {
                this.pageControl.updateLayout.call(this.pageControl, this.pageElement);
            }
        }

        private _hideElement(element) {
            if (element.winControl
                && element.winControl.viewController
                && element.winControl.viewController.onHidden) {
                element.winControl.viewController.onHidden();
            }
            this._addHiddenStyles(element);

            // Gracefully hide all visible dialogs
            Commerce.Controls.Dialog.DialogHandler.hideAll();

            this._enableAppBar(element, false);
        }

        private _addHiddenStyles(element) {
            element.style.position = "fixed";
            element.style.left = "-200000px";
            element.style.visibility = "hidden";
        }

        private _removeHiddenStyles(element) {
            element.style.visibility = "visible";
            element.style.position = "static";
            element.style.left = "";
        }

        private _showElement(element) {
            if (element) {
                this._removeHiddenStyles(element);
                var showAppBarOnLoad = true;

                if (element.winControl
                    && element.winControl.viewController
                    && element.winControl.viewController.onShown) {
                    element.winControl.viewController.onShown();
                    if (element.winControl.viewController.hasOwnProperty('showAppBarOnLoad')) {
                        showAppBarOnLoad = element.winControl.viewController.showAppBarOnLoad;
                    }

                }
                this._enableAppBar(element, true);
                if (showAppBarOnLoad) {
                    this._showAppBar(element);
                }
            }
        }

        private _enableAppBar(element: HTMLElement, appBarEnabled: boolean) {
            var appBar = element.querySelector("#commandAppBar");
            if (appBar) {
                appBar.winControl.disabled = (!appBarEnabled);
            }
        }

        private _showAppBar(element) {
            var appBar = element.querySelector("#commandAppBar");
            if (appBar) {
                if (appBar.winControl.disabled == false && Commerce.appBarAlwaysVisible()) {
                    appBar.winControl.show();
                }
            }
        }

        private _removeElement(removeElement, removeAll?: boolean) {
            // Do not remove element if it is an IKeepAliveView.
            if (!removeAll &&
                !ObjectExtensions.isNullOrUndefined(removeElement.winControl) &&
                !ObjectExtensions.isNullOrUndefined(removeElement.winControl.viewController) &&
                ObjectExtensions.isFunction(removeElement.winControl.viewController.keepAliveViewActivated)) {
                return;
            }

            // Dispose handlers that subscribed to global observables.
            var $removeElement = $(removeElement);
            if ($removeElement.hasClass(ViewNavigator._disposableObjectsTagName)) {
                var disposableObjects: IDisposable[] = <any>$removeElement.data(ViewNavigator._disposableObjectsTagName);

                while (disposableObjects.length > 0) {
                    var disposableObject: IDisposable = disposableObjects.pop();
                    ObjectExtensions.tryDispose(disposableObject);
                }

                // Clean up marker and data.
                $removeElement.removeClass(ViewNavigator._disposableObjectsTagName).removeData(ViewNavigator._disposableObjectsTagName);
            }

            if (!ObjectExtensions.isNullOrUndefined(removeElement.winControl)) {
                // Run unload.
                if (ObjectExtensions.isFunction(removeElement.winControl.unload)) {
                    removeElement.winControl.unload();
                }

                // Delay the dispose of controller to allow 'always' callbacks to finish on async results.
                setTimeout(() => {
                    ObjectExtensions.tryDispose(removeElement.winControl.viewController);
                }, 20000);

                // Dispose winjs control.
                WinJS.Utilities.disposeSubTree(removeElement);
                ObjectExtensions.tryDispose(removeElement.winControl);
            }

            // Remove KO subscriptions and references
            ko.removeNode(removeElement);
        }

        private _checkForAndRemoveNavigationCircle(newElement, oldElement) {
            // Get the first page in the history that isn't being shown. The top of the stack is always the current page.
            var firstPageInHistory = this._history[this._history.length - 2];

            // Check if the page we just navigated away from (oldElement) should be saved in th s history.
            // Check that we have a navigation circle in that the new page and the first page that isn't shown have the same uri.
            // If both of those conditions are met, we need to pop off the currentPage and the one before that to clear the circle.
            if (oldElement.winControl && oldElement.winControl.viewController && oldElement.winControl.viewController.saveInHistory == false && firstPageInHistory && newElement.winControl.uri == firstPageInHistory.winControl.uri) {
                this._removeElement(this._history.pop());
                this._removeElement(this._history.pop());
                this.navigationLog().pop();
                this.navigationLog().pop();
            }
        }

        /**
        *  This function updates application controls once a navigation
        *  has completed.
        */
        private _navigated(location: string, state?: any): void {
            // Do application specific on-navigated work here
            var backButton = this.backButton;
            if (backButton && this.isBackNavigationEnabled) {

                // Attaching directly instead addEventListener avoids multiple handler attach in case user navigate back to the view.
                backButton.onclick = () => {
                    this.navigateBack();
                };

                if (this._history.length > Commerce.ViewNavigator.NumberOfPagesInHistory) {
                    for (var i = 0; i < this._history.length; i++) {
                        if (Commerce.StringExtensions.isNullOrWhitespace($(this._history[i]).attr(ViewNavigator._stayAliveViewElementAttribute))) {
                            this._removeElement(this._history.splice(i, 1)[0]);
                            break;
                        }
                    }
                }

                // Enable back button if there are 2 or more pages in the history.
                if (this._history.length > 1) {
                    backButton.removeAttribute("disabled");
                } else {
                    backButton.setAttribute("disabled", "disabled");
                }
            }

            // Clear the history because this page doesn't have a back button.
            else {
                // Save the current page.
                var currentElement = this._history.pop();

                // Iterate over everything in the history, unload it, then remove it.
                var ele = this._history.pop();
                while (ele) {
                    this._removeElement(ele);

                    ele = this._history.pop();
                }

                // Add the current page back.
                this._history.push(currentElement);
            }

            var $element = $(this.pageElement);
            var disposableObjects: IDisposable[];

            // Additional handlers to observables later on navigation back and keep alive views.
            // We need to add them to already existing collections.
            if ($element.hasClass(ViewNavigator._disposableObjectsTagName)) {
                disposableObjects = <any>$element.data(ViewNavigator._disposableObjectsTagName);
            } else {
                disposableObjects = [];
                $element.addClass(ViewNavigator._disposableObjectsTagName).data(ViewNavigator._disposableObjectsTagName, disposableObjects);
            }

            // Get all global observables subscribers that are not yet tagged and attach to element.
            // Each subscribe handler will be disposed during removal.
            for (var property in Commerce.Session.instance) {
                var instanceProperty = Commerce.Session.instance[property];
                if (ko.isObservable(instanceProperty) && ArrayExtensions.hasElements(instanceProperty.w.change)) {
                    for (var i: number = instanceProperty.w.change.length - 1; i >= 0; i--) {
                        var changeHandler = instanceProperty.w.change[i];
                        if (changeHandler.taggedForDispose) {
                            // If we reached handler in the array that already have a tag, then we can break out of loop.
                            // The rest of the handlers should already be marked as well. 
                            break; 
                        }

                        disposableObjects.push(changeHandler);
                        changeHandler.taggedForDispose = true; // Tag handler to prevent full array scan on next navigation. 
                    }
                }
            }
        }

        /**
         *  Method for navigating back to the previous view
         */
        public navigateBack(): void {
            if (!this.isBackNavigationEnabled) {
                return;
            }
            
            // Get the page that was just showing, hide it and remove it.
            var currentElement = this.pageElement; 


            // Fires on navigate back event.
            if (currentElement.winControl && currentElement.winControl.viewController) {
                var onNavigateBack = currentElement.winControl.viewController["onNavigateBack"];
                if (!ObjectExtensions.isNullOrUndefined(onNavigateBack) && ObjectExtensions.isFunction(onNavigateBack)) {
                    // Cancels navigation on false.
                    var shouldContinue = onNavigateBack.call(currentElement.winControl.viewController);
                    if (!shouldContinue)
                        return;
                }    
            }

            UI.Tutorial.onBeforeNavigate();

            this._navigateBackInternal();
        }

        /**
        * Clear the navigation history from lastPageUrl to current page (exclusive).
        *
        * @param {string} lastPageUrl Last page in history stack to save.
        */
        private clearAllContent(): void {

            this._history.splice(0, this._history.length);

            // Remove everything from the contenthost.
            while (this.element.hasChildNodes()) {
                this._removeElement(this.element.lastChild, true);
            }

            // Remove everything tagged with IRemoveable. 
            $(document.body).find("[IRemoveable]").each((index, element) => {
                ko.removeNode(element);
            });

            // Add placeholder element.
            var pageElement = this._createPageElement();
            this.element.appendChild(pageElement);
            this._history.push(pageElement);
        }

        /**
         * Add handler for custom event for a current view. 
         * This is to ensure when node if removed all handlers for custom event are removed with it.
         *
         * @param {Element} element DOM element that will need to handle event.
         * @param {string} eventName Event name.
         * @param {any} eventHandler Event handler function.
         */
        public addEventHandler(element: Element, eventName: string, eventHandler: any): void {
            if (ObjectExtensions.isNullOrUndefined(element)) {
                throw new Error("Navigator.addEventHandler: element is a required parameter.");
            }

            if (StringExtensions.isNullOrWhitespace(eventName)) {
                throw new Error("Navigator.addEventHandler: eventName is a required parameter.");
            }

            if (ObjectExtensions.isNullOrUndefined(eventHandler)) {
                throw new Error("Navigator.addEventHandler: eventHandler is a required parameter.");
            }

            var $element = $(element);
            var globalEventHandlers: any = $element.data(ViewNavigator._globalEventsAttributeName);

            if (!globalEventHandlers) {
                globalEventHandlers = {};
            }

            // Init array in the event dictionary.
            if (!globalEventHandlers[eventName]) {
                globalEventHandlers[eventName] = [];
            }

            globalEventHandlers[eventName].push(eventHandler);

            $element.data(ViewNavigator._globalEventsAttributeName, globalEventHandlers);

            var removeHandler = () => {
                this.removeEventHandler(element, eventName, eventHandler);
            };

            // Clear event handlers for global event on element removal.
            $element.on("remove", removeHandler.bind(this));
        }

        /**
         * Removes event handlers for global events. This is in case controls on the view need to attach and detach events multiple times.
         *
         * @param {Element} element DOM element that will need to handle event.
         * @param {string} eventName Event name.
         * @param {any} eventHandler Event handler function for the event.
         */
        removeEventHandler(element: Element, eventName: string, eventHandler: any): void {
            var $element = $(element);
            var globalEventHandlers = $element.data(ViewNavigator._globalEventsAttributeName);
            if (globalEventHandlers) {
                // If node has event handlers for a given event add them to collection.
                var eventHandlers = globalEventHandlers[eventName];
                if (eventHandlers && eventHandlers.length > 0) {
                    // Remove handler if instance is found.
                    var index = eventHandlers.indexOf(eventHandler);
                    if (index >= 0) {
                        eventHandlers.splice(index, 1);
                    }
                }
            }
        }

        /**
         * Get event handlers for a given custom event. 
         *
         * @param {string} eventName Custom event name.
         * @returns Returns array of events in the order they were added.
         */
        public getHandlers(eventName: string): any[] {
            var handlers = [];
            $(document.body).find(":data(" + ViewNavigator._globalEventsAttributeName + ")").each((index: number, element: Element) => {
                var globalEventHandlers = $(element).data(ViewNavigator._globalEventsAttributeName);
                if (globalEventHandlers) {
                    var eventHandlers = globalEventHandlers[eventName];
                    if (eventHandlers && eventHandlers.length > 0) {
                        handlers = handlers.concat(eventHandlers);
                    }
                }
            });

            return handlers;
        }

        private _navigateBackInternal(): void {
            // Get the page that was just showing, hide it and remove it.
            var currentElement = this._history.pop();
            this.navigationLog().pop();
            this._hideElement(currentElement);
            this._removeElement(currentElement);

            // Get the previous page from this history and show it.
            var previousElement = this.pageElement;

            // Log the page view event on a best effort basis.
            var pageName: string = $(previousElement).attr("Action");
            if (!ObjectExtensions.isNullOrUndefined(pageName)) {
                RetailLogger.writePageViewEvent(pageName);
            }

            UI.Tutorial.onAfterNavigate(pageName, previousElement);
            this._showElement(previousElement);
        }
    }

    WinJS.Utilities.markSupportedForProcessing(ViewNavigator);
}
