/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Peripherals {
    "use strict";

    export type KeyboardPressEventHandler = (e: KeyboardEvent) => boolean;

    interface IKeyboardPressEventHandler {
        eventHandler: KeyboardPressEventHandler;
        priority: number;
    }

    export class KeyboardPressEventAggregator {
        private static _handlers: Array<IKeyboardPressEventHandler> = [];
        private static _isEventEnabled: boolean;

        /**
         * Adds a listner to keyboard event handler.
         * @param {KeyboardPressEvent} eventHandler The event handler.
         * @param {number} [priority] The priority of the event. Lower number will get priority. The default priority is 10.
         */
        public static addListener(eventHandler: KeyboardPressEventHandler, priority: number = 10): void {
            if (!ObjectExtensions.isFunction(eventHandler)) {
                return;
            }

            KeyboardPressEventAggregator._handlers.push(<IKeyboardPressEventHandler>{ eventHandler: eventHandler, priority: priority });
            KeyboardPressEventAggregator._handlers.sort((a: IKeyboardPressEventHandler, b: IKeyboardPressEventHandler) => { return a.priority - b.priority; });

            if (!KeyboardPressEventAggregator._isEventEnabled) {
                document.addEventListener("keypress", KeyboardPressEventAggregator.scanHandler, true);
                KeyboardPressEventAggregator._isEventEnabled = true;
            }
        }

        /**
         * Removes a listner to keyboard event handler.
         * @param {KeyboardPressEvent} eventHandler The event handler.
         */
        public static removeListner(eventHandler: KeyboardPressEventHandler): void {
            for (var i in KeyboardPressEventAggregator._handlers) {
                if (KeyboardPressEventAggregator._handlers[i].eventHandler === eventHandler) {
                    KeyboardPressEventAggregator._handlers.splice(i, 1);
                }
            }

            if (KeyboardPressEventAggregator._isEventEnabled && KeyboardPressEventAggregator._handlers.length === 0) {
                document.removeEventListener("keypress", KeyboardPressEventAggregator.scanHandler);
                KeyboardPressEventAggregator._isEventEnabled = false;
            }
        }

        public static isInputField(keyPressedSourceElement: any): boolean {
            return keyPressedSourceElement.tagName.toLowerCase() === "textarea" || (keyPressedSourceElement.tagName.toLowerCase() === "input"
                                                                                    && (keyPressedSourceElement.type.toLowerCase() === "text"
                                                                                    || keyPressedSourceElement.type.toLowerCase() === "password"
                                                                                    || keyPressedSourceElement.type.toLowerCase() === "search"
                                                                                    || keyPressedSourceElement.type.toLowerCase() === "number"
                                                                                    || keyPressedSourceElement.type.toLowerCase() === "email"
                                                                                    || keyPressedSourceElement.type.toLowerCase() === "url"));
        }

        private static scanHandler(e: KeyboardEvent): boolean {
            // Call each event listner;
            for (var i in KeyboardPressEventAggregator._handlers) {
                if (!KeyboardPressEventAggregator._handlers[i].eventHandler(e)) {
                    // Remove the focus from target element to avoid character showing/handling on elements.
                    (<HTMLElement>e.target).blur();
                    document.body.focus();

                    // Makes sure the event does not bubble up.
                    e.stopImmediatePropagation();

                    // Non-IE browsers implement DOM 2 specification for eating char
                    e.preventDefault();

                    return false;  // eat char
                }
            }

            return true; // Don't handle
        }
    }
}
