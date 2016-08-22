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

    export class JQueryUITouchExtensions {

        private static touchHandled: boolean;
        private static touchMoved: boolean;

        /**
         * Enables touch event simulation for a given JQuery element.
         * @param {JQuery} element The JQuery element.
         */
        public static enableTouchEmulation(element: JQuery): void {
            element.on({
                touchstart: JQueryUITouchExtensions.onTouchStart,
                touchmove: JQueryUITouchExtensions.onTouchMove,
                touchend: JQueryUITouchExtensions.onTouchEnd
            });
        }

        /**
         * Disables touch event simulation for a given JQuery element.
         * @param {JQuery} element The JQuery element.
         */
        public static disableTouchEmulation(element: JQuery): void {
            element.off({
                touchstart: JQueryUITouchExtensions.onTouchStart,
                touchmove: JQueryUITouchExtensions.onTouchMove,
                touchend: JQueryUITouchExtensions.onTouchEnd
            });
        }

        /**
         * Simulate a mouse event based on a corresponding touch event.
         * @param {Object} event A touch event
         * @param {String} simulatedType The corresponding mouse event.
         */
        private static simulateMouseEvent(event: JQueryEventObject, simulatedType: string): void {

            var touchEvent: TouchEvent = <TouchEvent>event.originalEvent;

            // Ignoring multi-touch events.
            if (touchEvent.touches.length > 1) {
                return;
            }

            event.preventDefault();

            var touch: Touch = touchEvent.changedTouches[0];
            var simulatedEvent: MouseEvent = <MouseEvent>document.createEvent("MouseEvents");

            simulatedEvent.initMouseEvent(
                simulatedType,    // type
                true,             // bubbles
                true,             // cancelable
                window,           // view
                1,                // detail
                touch.screenX,    // screenX
                touch.screenY,    // screenY
                touch.clientX,    // clientX
                touch.clientY,    // clientY
                false,            // ctrlKey
                false,            // altKey
                false,            // shiftKey
                false,            // metaKey
                0,                // button
                null              // relatedTarget
            );

            // Dispatch the simulated event to the target element
            event.target.dispatchEvent(simulatedEvent);
        }

        private static onTouchStart(event: JQueryEventObject): void {

            // Ignore the event if touch already being handled by another widget.
            if (this.touchHandled) {
                return;
            }

            // Set the flag to prevent other widgets from inheriting the touch event.
            JQueryUITouchExtensions.touchHandled = true;

            // Track movement to determine if interaction was a click
            JQueryUITouchExtensions.touchMoved = false;

            JQueryUITouchExtensions.simulateMouseEvent(event, "mouseover");
            JQueryUITouchExtensions.simulateMouseEvent(event, "mousemove");
            JQueryUITouchExtensions.simulateMouseEvent(event, "mousedown");
        }

        private static onTouchMove(event: JQueryEventObject): void {
            // Ignore event if not handled
            if (!JQueryUITouchExtensions.touchHandled) {
                return;
            }

            // Interaction was not a click
            JQueryUITouchExtensions.touchMoved = true;

            // Simulate the mousemove event
            JQueryUITouchExtensions.simulateMouseEvent(event, "mousemove");
        }

        private static onTouchEnd(event: JQueryEventObject): void {

            // Ignore event if not handled.
            if (!JQueryUITouchExtensions.touchHandled) {
                return;
            }

            JQueryUITouchExtensions.simulateMouseEvent(event, "mouseup");
            JQueryUITouchExtensions.simulateMouseEvent(event, "mouseout");

            // If the touch interaction did not move, it should trigger a click.
            if (!JQueryUITouchExtensions.touchMoved) {
                JQueryUITouchExtensions.simulateMouseEvent(event, "click");
            }

            // Unset the flag to allow other widgets to inherit the touch event.
            JQueryUITouchExtensions.touchHandled = false;
        }
    }
}