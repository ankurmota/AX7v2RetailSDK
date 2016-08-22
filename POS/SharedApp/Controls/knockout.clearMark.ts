/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Controls {
    "use strict";

    /**
     * Custom clearMark binding.
     * This binding is dependent on DynamicsFont.
     * Used for input type search to provide custom clear mark.
     * Example: <input type="search" data-bind="clearMark: true"/>
     */
    class ClearMark {

        private static markShift: number = 5;
        private static paddingLeftClass: string = "padding-left";
        private static paddingRightClass: string = "padding-right";
        private static fontColorClass: string = "color";
        private static fontSizeClass: string = "font-size";
        private static wrapperCssClass: string = "search-wrapper grow";
        private static clearMarkCssClass: string = "clear-mark";
        private static divElement: string = "<div />";
        private static closeMarkDynamicsFont: string = "&#xE10A";


        public static init(element: HTMLElement, valueAccessor: any, allBindingsAccessor: any, viewModel: any, bindingContext: KnockoutBindingContext): void {
            var $element: JQuery = $(element);
            var fontSize: string = $element.css(ClearMark.fontSizeClass);
            var originalPadding: string = $element.css(ClearMark.paddingRightClass);
            var $wrapper: JQuery = $(ClearMark.divElement, {
                "class": ClearMark.wrapperCssClass
            });
            $element.wrap($wrapper);

            var $clearMark: JQuery = $(ClearMark.divElement)
                .html(ClearMark.closeMarkDynamicsFont)
                .addClass(ClearMark.clearMarkCssClass)
                .css(ClearMark.fontSizeClass, fontSize)
                .appendTo($element.parent());

            if (StringExtensions.isEmpty($element.val())) {
                $clearMark.hide();
            }

            var valueBinding: any = allBindingsAccessor().value;
            var value: string = ko.utils.unwrapObservable(valueBinding);
            if (value && !StringExtensions.isEmpty(value)) {
                ClearMark.show($element, $clearMark, fontSize);
            }

            $element.on("input change", () => {
                if ($element.val() && $element.val().length > 0) {
                    // Update clear mark visibility.
                    ClearMark.show($element, $clearMark, fontSize);
                } else {
                    ClearMark.hide($element, $clearMark, originalPadding);
                }
            });

            $clearMark.on("click", (clickEvent: JQueryEventObject) => {
                if (ko.isObservable(valueBinding)) {
                    valueBinding(StringExtensions.EMPTY);
                } else {
                    $element.val(StringExtensions.EMPTY);
                }

                $clearMark.hide();
                $element.focus();
                clickEvent.stopPropagation();
            });
        }

        /**
         * Updates the clear mark styles and shows it. 
         * @param $element  Input element
         * @param $clearMark clearMark element
         * @param fontSize font size of the element
         * @param orginalPadding original padding of input.
         * @returns {} 
         */
        private static show($element: JQuery, $clearMark: JQuery, fontSize: string): void {
            // get em size
            var em: number = parseFloat($("body").css(ClearMark.fontSizeClass));

            var fontPadding: number = parseInt(fontSize, null);
            // padding of the input is 1 em + base padding + font size of clear mark.
            var elementPadding: number = em + ClearMark.markShift + fontPadding;
            var paddingClass: string = !CSSHelpers.isRightToLeft() ? ClearMark.paddingRightClass : ClearMark.paddingLeftClass;
            var fontColor: string = $element.css(ClearMark.fontColorClass);
            $clearMark.css(ClearMark.fontColorClass, fontColor);
            $element.css(paddingClass, elementPadding + "px");
            $clearMark.show();
        }

        /**
         * Hides the clear mark and clears input paddings.
         * @param $element Input element
         * @param $clearMark Clear mark element
         * @param originalPadding original padding of input
         * @returns {} 
         */
        private static hide($element: JQuery, $clearMark: JQuery, originalPadding: string): void {
            $element.css(ClearMark.paddingRightClass, originalPadding);
            $element.css(ClearMark.paddingLeftClass, originalPadding);
            $clearMark.hide();
        }
    }

    ko.bindingHandlers.clearMark = ClearMark;
}
