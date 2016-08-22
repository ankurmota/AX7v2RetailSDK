/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../Utilities/ThrowIf.ts" />
/// <reference path="Configuration/ITaskRecorderConfig.ts" />
/// <reference path="ITaskSubscriber.ts" />

module Commerce.TaskRecorder {

    declare type ActionHandler = (actions: Configuration.ITaskRecorderEventAction[]) => void;

    /**
     * Listener of events for Task Recorder.
     */
    export class TaskRecorderEventListener implements IDisposable {

        private static dataAxBubbleAttribute: string = "data-ax-bubble";
        private static roleAttribute: string = "role";
        private static typeAttribute: string = "type";
        private static dataActionAttribute: string = "data-action";
        private static disabledAttribute: string = "disabled";

        private subscriber: ITaskSubscriber;
        private config: Configuration.ITaskRecorderConfig;

        private eventDictionary: Dictionary<EventListener[]>;

        private lockKeyDown: boolean;

        /**
         * Constructor.
         * @param {ITaskSubscriber} subscriber Subscriber for event listener.
         * @param {Configuration.ITaskRecorderConfig} config Task Recorder configuration.
         */
        constructor(subscriber: ITaskSubscriber, config: Configuration.ITaskRecorderConfig) {
            ThrowIf.argumentIsNotObject(subscriber, "subscriber");
            ThrowIf.argumentIsNotObject(config, "config");

            this.subscriber = subscriber;
            this.config = config;
            this.eventDictionary = new Dictionary<EventListener[]>();
            this.subscribeToGlobalEvents();

            this.lockKeyDown = false;
        }

        /**
         * Clean up.
         */
        public dispose(): void {
            this.removeCurrentEventListeners();
        }

        private removeCurrentEventListeners(): void {
            this.config.events.forEach((event: Configuration.ITaskRecorderEvent) => {
                if (!ObjectExtensions.isNullOrUndefined(event.custom) &&
                    ArrayExtensions.hasElements(event.custom.internalEvents)) {
                    this.removeElementEventListener(event.custom.internalEvents);
                }
            });

            this.eventDictionary.forEach((name, listeners) => {
                listeners.forEach((listener: EventListener) => {
                    window.removeEventListener(name, listener, true);
                });
            });
        }

        /**
         * Remove listenets for internal events.
         */
        private removeElementEventListener(internalEvents: Configuration.ITaskRecorderElementEvent[]) {
            if (ArrayExtensions.hasElements(internalEvents)) {
                internalEvents.forEach(event => {
                    if (!ObjectExtensions.isNullOrUndefined(event.element) &&
                        !ObjectExtensions.isNullOrUndefined(event.listener) &&
                        !StringExtensions.isNullOrWhitespace(event.name)) {
                        event.element.removeEventListener(event.name, event.listener, true);
                    }
                });
            }
        }

        private subscribeToGlobalEvents(): void {
            this.config.events.forEach((event: Configuration.ITaskRecorderEvent) => {
                if (!ObjectExtensions.isNullOrUndefined(event.custom)) {
                    this.subscribeToCustomEvent(event);
                } else {
                    this.subscribeToEvent(event.eventName, event.rules);
                }
            });

            // unlock of keydown event
            if (this.eventDictionary.hasItem("keydown")) {
                this.addEventListener("keyup", () => { this.lockKeyDown = false; });
            }
        }

        /**
         * Add event listener for event.
         *
         * @param {string} eventName Event name.
         * @param {Configuration.ITaskRecorderEventRule[]} eventRules Event rules.
         */
        private subscribeToEvent(eventName: string, eventRules: Configuration.ITaskRecorderEventRule[]): void {
            this.processEvents([eventName], eventRules, (actions: Configuration.ITaskRecorderEventAction[]) => {
                if (ArrayExtensions.hasElements(actions)) {
                    var deferred: boolean = false;

                    actions.forEach((action: Configuration.ITaskRecorderEventAction) => {
                        deferred = deferred || action.deferred;
                    });

                    if (deferred) {
                        // place the event action handling at the end of the event queue
                        setTimeout(() => {
                            this.handleActions(actions);
                        }, 0);
                    } else {
                        this.handleActions(actions);
                    }
                }
            });
        }

        /**
         * Subscribe to complex WinJS events: invoke, checked.
         * We cannot properly handle these complex WinJS events so we hadle them as composite events: mousedown, mouseleave, mouseup, mouseenter.
         */
        private subscribeToCustomEvent(event: Configuration.ITaskRecorderEvent) {

            var runAction: boolean;

            // run the algorithm, when one of the startEvents is triggered
            this.processEvents(event.custom.startEvents, event.rules, (actions: Configuration.ITaskRecorderEventAction[]) => {
                if (event.custom.startPhase) {
                    return;
                }

                event.custom.actions = actions;

                actions.forEach((action: Configuration.ITaskRecorderEventAction) => {
                    runAction = !ObjectExtensions.isNullOrUndefined(action.element) && event.custom.runAction;
                    event.custom.startPhase = true;

                    if (!ObjectExtensions.isNullOrUndefined(action.element) && !ObjectExtensions.isNullOrUndefined(event.custom.elementEvents)) {
                        event.custom.internalEvents = event.custom.internalEvents || [];

                        // subscribe to events and set runAction, when one of them triggered on the element
                        if (ArrayExtensions.hasElements(event.custom.elementEvents.acceptEvents)) {
                            this.addElementEvents(
                                action.element,
                                event.custom.elementEvents.acceptEvents,
                                event.custom.internalEvents,
                                () => {
                                    runAction = true;
                                });
                        }

                        // subscribe to events and reset runAction, when one of them triggered on the element
                        if (ArrayExtensions.hasElements(event.custom.elementEvents.declineEvents)) {
                            this.addElementEvents(
                                action.element,
                                event.custom.elementEvents.declineEvents,
                                event.custom.internalEvents,
                                () => {
                                    runAction = false;
                                });
                        }
                    }
                });
            });

            // stop the algorithm, when one of the endEvents is triggered
            this.processEvents(event.custom.endEvents, event.rules, (actions: Configuration.ITaskRecorderEventAction[]) => {
                if (!event.custom.startPhase) {
                    return;
                }

                event.custom.startPhase = false;

                setTimeout(() => {
                    if (!ObjectExtensions.isNullOrUndefined(event.custom.actions) && runAction) {
                        this.handleActions(event.custom.actions);
                    }

                    if (ArrayExtensions.hasElements(event.custom.internalEvents)) {
                        this.removeElementEventListener(event.custom.internalEvents);
                        event.custom.internalEvents = [];
                    }

                    event.custom.actions = null;
                }, 0);
            });
        }

        /**
         * Add internal events to custom event
         */
        private addElementEvents(element: HTMLElement, eventNames: string[], elementEvents: Configuration.ITaskRecorderElementEvent[], listener: EventListener) {
            eventNames.forEach(event => {
                element.addEventListener(event, listener, true);
                elementEvents.push({
                    element: element,
                    listener: listener,
                    name: event
                });
            });
        }

        /**
         * Create listeners for events and subscribe them
         */
        private processEvents(eventNames: string[], eventRules: Configuration.ITaskRecorderEventRule[], actionHandler: ActionHandler) {
            var listener = this.getEventListener(eventRules, actionHandler);
            eventNames.forEach(eventName => {
                this.addEventListener(eventName, listener);
            });
        }

        private getEventListener(eventRules: Configuration.ITaskRecorderEventRule[], actionHandler: ActionHandler): EventListener {
            return ((event: Event) => {
                if (StringExtensions.compare(event.type, "keydown", true) === 0) {
                    if (this.lockKeyDown) {
                        return;
                    }

                    this.lockKeyDown = true;
                }

                var actions: Configuration.ITaskRecorderEventAction[] = this.applyRules(event, eventRules);

                if (ArrayExtensions.hasElements(actions)) {
                    actionHandler(actions);
                }
            });
        }

        private addEventListener(eventName: string, listener: EventListener) {
            window.addEventListener(eventName, listener, true);

            if (!this.eventDictionary.hasItem(eventName)) {
                this.eventDictionary.setItem(eventName, []);
            }

            this.eventDictionary.getItem(eventName).push(listener);
        }

        private applyRules(event: Event, rules: Configuration.ITaskRecorderEventRule[]): Configuration.ITaskRecorderEventAction[] {
            var element: HTMLElement = <HTMLElement>event.target;
            var resultRule: Configuration.ITaskRecorderEventRule = null;
            var matchElement: HTMLElement = null;
            var results: Configuration.ITaskRecorderEventAction[] = [];

            rules.forEach((rule: Configuration.ITaskRecorderEventRule) => {
                if (!ObjectExtensions.isNullOrUndefined(resultRule)) {
                    return;
                }

                matchElement = this.applyRule(event, element, rule);

                if (!ObjectExtensions.isNullOrUndefined(matchElement)) {
                    resultRule = rule;
                }
            });

            if (!ObjectExtensions.isNullOrUndefined(resultRule) && !resultRule.ignore) {
                results.push({
                    strategies: resultRule.actionData.strategies,
                    text: resultRule.actionData.text,
                    defaultValue: resultRule.actionData.defaultValue,
                    element: matchElement,
                    deferred: resultRule.actionData.deferred,
                    compositionOrder: resultRule.actionData.compositionOrder
                });

                results = results.concat(this.parentHandler(event, matchElement, resultRule));
                results = results.concat(this.childHandler(event, matchElement, resultRule));
            }

            return results;
        }

        /**
         * Handles rules of parent element.
         *
         * @param {Event} event Handling event.
         * @param {HTMLElement} element Handling element.
         * @param {Configuration.ITaskRecorderEventRule} rule Handling rule.
         * @returns {Configuration.ITaskRecorderEventAction[]} Handled actions.
         */
        private parentHandler(event: Event, element: HTMLElement, rule: Configuration.ITaskRecorderEventRule): Configuration.ITaskRecorderEventAction[] {
            var results: Configuration.ITaskRecorderEventAction[] = [];

            if (!ObjectExtensions.isNullOrUndefined((rule.handleParent))) {
                var parentMatchElement = this.applyRuleToParent(event, element, rule.handleParent);
                if (!ObjectExtensions.isNullOrUndefined(parentMatchElement)) {
                    var resultEventAction: Configuration.ITaskRecorderEventAction = {
                        strategies: rule.handleParent.actionData.strategies,
                        text: rule.handleParent.actionData.text,
                        defaultValue: rule.handleParent.actionData.defaultValue,
                        element: parentMatchElement,
                        deferred: rule.handleParent.actionData.deferred,
                        compositionOrder: rule.handleParent.actionData.compositionOrder
                    };

                    results.push(resultEventAction);
                    results = results.concat(this.parentHandler(event, parentMatchElement, rule.handleParent));
                    results = results.concat(this.childHandler(event, parentMatchElement, rule.handleParent));
                }
            }

            return results;
        }

        /**
         * Handles rules of child elements.
         *
         * @param {Event} event Handling event.
         * @param {HTMLElement} element Handling element.
         * @param {Configuration.ITaskRecorderEventRule} rule Handling rule.
         * @returns {Configuration.ITaskRecorderEventAction[]} Handled actions.
         */
        private childHandler(event: Event, element: HTMLElement, rule: Configuration.ITaskRecorderEventRule): Configuration.ITaskRecorderEventAction[] {
            if (ObjectExtensions.isNullOrUndefined(rule.handleChild)) {
                return [];
            }

            var results: Configuration.ITaskRecorderEventAction[] = [];

            for (var i: number = 0; i < element.children.length; i++) {
                var matchElement: HTMLElement = this.applyRule(event, <HTMLElement>element.children[i], rule.handleChild);

                if (!ObjectExtensions.isNullOrUndefined(matchElement)) {
                    results.push({
                        strategies: rule.handleChild.actionData.strategies,
                        text: rule.handleChild.actionData.text,
                        defaultValue: rule.handleChild.actionData.defaultValue,
                        element: matchElement,
                        deferred: rule.handleChild.actionData.deferred,
                        compositionOrder: rule.handleChild.actionData.compositionOrder
                    });
                }

                results = results.concat(this.childHandler(event, <HTMLElement>element.children[i], rule));
            }

            return results;
        }

        private applyRule(event: Event, element: HTMLElement, rule: Configuration.ITaskRecorderElementRule): HTMLElement {
            var match: boolean = false;
            var checked: boolean = false;
            var resultElement: HTMLElement = element;

            if (ObjectExtensions.isNullOrUndefined(element) ||
                !ObjectExtensions.isNullOrUndefined(element.attributes[TaskRecorderEventListener.disabledAttribute])) {
                return null;
            }

            // check for keyboard event Backspace is "Go To Back"
            var backspaceKeyCode = 8;
            if (NumberExtensions.compare(rule.keyCode, (<KeyboardEvent>event).keyCode) === 0
                && NumberExtensions.compare(rule.keyCode, backspaceKeyCode) === 0) {
                return resultElement;
            }

            // check for tag
            if (ObjectExtensions.isNullOrUndefined(rule.tagName) || StringExtensions.compare(rule.tagName, element.tagName, true) === 0) {
                // check for ids
                if (ArrayExtensions.hasElements(rule.ids)) {
                    match = (ArrayExtensions.hasElement(rule.ids, element.id));
                    checked = true;
                }
                // check for classes
                if (!match && ArrayExtensions.hasElements(rule.classNames)) {
                    rule.classNames.forEach((className: string) => {
                        match = (element.classList.contains(className));
                    });
                    checked = true;
                }
                // check for roles
                if (!match && ArrayExtensions.hasElements(rule.roles)) {
                    var role = element.getAttribute(TaskRecorderEventListener.roleAttribute);
                    match = role ? ArrayExtensions.hasElement(rule.roles, role.toLowerCase()) : false;
                    checked = true;
                }
                // check for type attributes
                if (!match && ArrayExtensions.hasElements(rule.types)) {
                    var type = element.getAttribute(TaskRecorderEventListener.typeAttribute);
                    match = type ? ArrayExtensions.hasElement(rule.types, type.toLowerCase()) : false;
                    checked = true;
                }
                // check for type data-ax-bubble
                if (!match && !StringExtensions.isNullOrWhitespace(rule.dataAxBubble)) {
                    if (!ObjectExtensions.isNullOrUndefined(element.attributes[TaskRecorderEventListener.dataAxBubbleAttribute])) {
                        match = StringExtensions.compare(rule.dataAxBubble, element.attributes[TaskRecorderEventListener.dataAxBubbleAttribute].value, true) === 0;
                    }

                    checked = true;
                }
                // check for data-action
                if (!match && !ObjectExtensions.isNullOrUndefined(rule.dataAction)) {
                    if (!ObjectExtensions.isNullOrUndefined(element.attributes[TaskRecorderEventListener.dataActionAttribute])) {
                        match = NumberExtensions.compare(rule.dataAction, element.attributes[TaskRecorderEventListener.dataActionAttribute].value) === 0;
                    }

                    checked = true;
                }

                // check for keyCode
                if (match && (StringExtensions.compare(event.type, "keypress", true) === 0 ||
                    StringExtensions.compare(event.type, "keydown", true) === 0 ||
                    StringExtensions.compare(event.type, "keyup", true) === 0)) {
                    if (!ObjectExtensions.isNullOrUndefined(rule.keyCode)) {
                        match = NumberExtensions.compare(rule.keyCode, (<KeyboardEvent>event).keyCode) === 0;
                    } else {
                        match = false;
                    }
                    checked = true;
                }

                if (!checked) {
                    // rule hasn't any class, any id, any data-ax-bubble and any role
                    match = true;
                }
            }

            if (match && !ObjectExtensions.isNullOrUndefined(rule.parent)) {
                match = !ObjectExtensions.isNullOrUndefined(this.applyRuleToParent(event, element, rule.parent));
            }

            //check for parent's chain
            if (!match && rule.checkParents && StringExtensions.compare("body", element.tagName, true) !== 0) {
                resultElement = this.applyRuleToParent(event, element, rule);
                if (!ObjectExtensions.isNullOrUndefined(resultElement)) {
                    match = true;
                }
            }

            return match ? resultElement : null;
        }

        private applyRuleToParent(event: Event, element: HTMLElement, rule: Configuration.ITaskRecorderElementRule): HTMLElement {
            var parent = element.parentElement;
            return this.applyRule(event, parent, rule);
        }

        private handleActions(actions: Configuration.ITaskRecorderEventAction[]): void {
            var actionText: string = this.getActionsDescription(actions);

            var step: Commerce.Model.Entities.UserAction = {
                Id: StringExtensions.EMPTY,
                Description: actionText
            };

            this.subscriber.addStep(step);
            Commerce.RetailLogger.taskRecorderHandleAction(actionText);
        }

        /**
         * Gets step description for actions.
         *
         * @param {Configuration.ITaskRecorderEventAction[]} actions Handled actions.
         * @returns {string} The step description.
         */
        private getActionsDescription(actions: Configuration.ITaskRecorderEventAction[]): string {
            if (!ArrayExtensions.hasElements(actions)) {
                return StringExtensions.EMPTY;
            }

            // filter and order actions in case of composition
            if (actions.length > 1) {
                // remove non-composition elements
                actions = actions.filter((action: Configuration.ITaskRecorderEventAction) => {
                    return !ObjectExtensions.isNullOrUndefined(action.compositionOrder);
                });

                actions = actions.sort((a: Configuration.ITaskRecorderEventAction, b: Configuration.ITaskRecorderEventAction) => {
                    if (a.compositionOrder < b.compositionOrder) {
                        return -1;
                    } else if (a.compositionOrder > b.compositionOrder) {
                        return 1;
                    }

                    return 0;
                });
            }

            // prepare list of formatted descriptions
            var descriptionComponents: string[] = [];
            actions.forEach((action: Configuration.ITaskRecorderEventAction): void => {
                descriptionComponents.push(this.getActionDescription(action));
            });

            var actionDescription: string = StringExtensions.EMPTY;

            if (!ArrayExtensions.hasElements(descriptionComponents)) {
                return StringExtensions.EMPTY;
            } else if (descriptionComponents.length === 1) {
                return descriptionComponents[0];
            }

            var formatString: string = ViewModelAdapter.getResourceString("string_10129"); // "{0} {1}"
            actionDescription = StringExtensions.format(formatString, descriptionComponents[0], descriptionComponents[1]);
            for (var i: number = 2; i < descriptionComponents.length; ++i) {
                actionDescription = StringExtensions.format(formatString, actionDescription, descriptionComponents[i]);
            }

            return actionDescription;
        }

        private getActionDescription(action: Configuration.ITaskRecorderEventAction): string {
            var value: string = StringExtensions.EMPTY;

            if (ArrayExtensions.hasElements(action.strategies)) {
                for (var indexStrategy = 0; indexStrategy < action.strategies.length; indexStrategy++) {
                    value = this.applyActionDataStrategy(action.element, action.strategies[indexStrategy]);
                    if (value) {
                        break;
                    }
                }
            }

            var parameter = value || Commerce.ViewModelAdapter.getResourceString(action.defaultValue);
            value = parameter ? StringExtensions.format(Commerce.ViewModelAdapter.getResourceString(action.text), parameter) : Commerce.ViewModelAdapter.getResourceString(action.text);

            return value;
        }

        private applyActionDataStrategy(element: HTMLElement, strategy: string): string {
            switch (strategy) {
                case "content":
                    if (!StringExtensions.isNullOrWhitespace(element.innerHTML)) {
                        return element.innerText.trim();
                    }
                    break;
                case "value":
                    return (<HTMLInputElement>element).value;
                    break;
                case "label":
                    if (!StringExtensions.isNullOrWhitespace(element.id)) {
                        var label = $("label[for='" + element.id + "']");
                        if (label[0]) {
                            return label[0].innerHTML;
                        }
                    }
                    break;
                case "title":
                    if (!StringExtensions.isNullOrWhitespace(element.title)) {
                        return element.title;
                    }
                    break;
                case "aria-label":
                    if (!ObjectExtensions.isNullOrUndefined(element.attributes["aria-label"])) {
                        return element.attributes["aria-label"].value;
                    }
                    break;
                case "actionProperty":
                    var $element: JQuery = $(element);
                    var elementData = $element.data("commerceButtonGridButtonOptions");

                    if (!ObjectExtensions.isNullOrUndefined(elementData)) {
                        var action: number = (<Proxy.Entities.ButtonGridButton>elementData).Action;
                        var actionProperty: any = (<Proxy.Entities.ButtonGridButton>elementData).ActionProperty;

                        if (action === Commerce.Proxy.Entities.RetailOperation.TotalDiscountPercent
                            && !StringExtensions.isNullOrWhitespace(actionProperty)
                            && !NumberExtensions.isNullOrZero(actionProperty)) {
                            return StringExtensions.format(ViewModelAdapter.getResourceString('string_10089'), actionProperty);
                        }
                    }
                    break;
            }

            return null;
        }
    }
}