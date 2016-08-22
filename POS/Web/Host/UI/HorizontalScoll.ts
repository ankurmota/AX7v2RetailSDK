/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Host.UI {
    "use strict";

   /**
    * Allows to scroll in chrome without holding shift.
    */
    export class HorizontalScoll {

        private static overflowY: string = "overflow-y";
        private static overflowX: string = "overflow-x";

       /**
        * Attaches the scroll event to dom.
        * In a loop goes from event target to html body. 
        * Breaks on first vertically scrollable element or scrolls horizontally scrollabe element. 
        */
        public static init(): void {
            $(document).on("mousewheel", (event: any) => {
                // This is the case when you are using mouse with horizontal scroll or touchpad.
                if (event.originalEvent.deltaX > 0 || event.ctrlKey || event.shiftKey) {
                    return;
                }

                var delta: number = event.originalEvent.deltaY;
                var target: Element = event.target;
                while (!ObjectExtensions.isNullOrUndefined(target)) {

                    let overflowY: string = $(target).css(HorizontalScoll.overflowY);
                    let overflowX: string = $(target).css(HorizontalScoll.overflowX);

                    // If the target has vertical scroll we let browser to scroll.
                    if (target.scrollHeight > target.clientHeight &&
                        (overflowY === "scroll" || overflowY === "auto")) {
                        break;
                    }

                    // Check if the target has horizontal scroll.
                    if (target.scrollWidth > target.clientWidth &&
                        (overflowX === "scroll" || overflowX === "auto")) {
                        var textDirection: string = $("body").attr("dir");
                        delta = textDirection === "ltr" ? delta : -delta;
                        target.scrollLeft += delta;
                        event.preventDefault();
                        break;
                    } else {

                        let $target: JQuery = $(target).parent();
                        if (!$target.is("body")) {
                            target = $target.get(0);
                        } else {
                            break;
                        }
                    }
                }
            });
        }
    }
}