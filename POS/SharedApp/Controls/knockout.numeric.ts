/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/*
 * numeric knockout binding extensions
 * 
 * Limits the characters allowed for entering into the text box. 
 * Created because IE type="number" allows entering non numeric characters
 *
 * Usage: data-bind="numeric: true, ..." 
 */
module Commerce {
    "use strict";

    ko.bindingHandlers.numeric = {
        init: (element: HTMLElement): void => {
            var previousValue: string = StringExtensions.EMPTY;
            var $element: JQuery = $(element);
            $element.on("keydown", (event: any) => {
                // Allow: backspace, delete, tab, escape, and enter 
                if (event.keyCode === 46 || event.keyCode === 8 || event.keyCode === 9 || event.keyCode === 27 || event.keyCode === 13 ||
                    // Allow: -
                    event.keyCode === 109 ||
                    // Allow: Ctrl+A, Ctrl+C, Ctrl+V
                    ((event.keyCode === 65 || event.keyCode === 67 || event.keyCode === 86) && event.ctrlKey === true) ||
                    // Allow: . ,
                    (event.keyCode === 188 || event.keyCode === 190 || event.keyCode === 110) ||
                    // Allow: home, end, left, right
                    (event.keyCode >= 35 && event.keyCode <= 39)) {
                    // let it happen, don't do anything
                    return;
                } else {
                    // Ensure that it is a number and stop the keypress
                    if (event.shiftKey || (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {
                        event.preventDefault();
                    }
                }
            });
            // stores previos value to use it if paste fails.
            $element.on("keyup", (event: any) => {
                previousValue = $element.val();
            });

            // prevent paste if the new value is not a number
            $element.on("paste", (pasteEvent: JQueryEventObject) => {
                var newValue: string;
                var insertStart: number = (<any>element).selectionStart;
                var insertEnd: number = (<any>element).selectionEnd;
                var pastedValue: string = (<any>window).clipboardData.getData("text");
                if (previousValue.length > insertStart) {
                    newValue = previousValue.substr(0, insertStart)
                        + pastedValue
                        + previousValue.substr(insertEnd, previousValue.length);
                } else {
                    newValue = previousValue + pastedValue;
                }
                var numeric: number = Number(newValue);
                if (isNaN(numeric)) {
                    $element.val(previousValue);
                    pasteEvent.preventDefault();
                } else {
                    previousValue = newValue;
                }
            });
        }
    };
}