/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce {
    "use strict";

    export var appBarAlwaysVisible: Observable<boolean> = ko.observable(true);
    export var isAppBarVisible: () => boolean = () => { return !ObjectExtensions.isNullOrUndefined($("#commandAppBar").get(0)); };

    appBarAlwaysVisible.subscribe((newValue: any) => {
        // update app bar visibility in local storage
        ApplicationStorage.setItem(ApplicationStorageIDs.APP_BAR_ALWAYS_VISIBLE, StringExtensions.EMPTY + newValue);
    });

   /*
    * Extends the functionality of the WinJS.UI.AppBar control by allowing the app bar to
    * be configured to be always visible in the app and returns the modified WinJS AppBar control
    */
    export class AppBarControl {

        public appBar: any;
        private _element: Element;
        private _options: any;
        private _parentViewPath: string;

       /**
        * @param {Element} element - the HTML element being bound to the AppBarControl
        * @param {any} options - options passed to the AppBarControl through the win-data-options attribute.
        *                        these are simply passed through to the WinJS.UI.AppBar that is created in this control
        *                        and can handle any option supported by the WinJS.UI.AppBar control.
        * @return {AppBarControl} returns the AppBarControl which extends the WinJS.UI.AppBar with extra functionality
        *                         to support visibility settings.
        */
        constructor(element: HTMLElement, options: any) {
            // store parent page path info to hide the app bar when navigating away from the page
            this._parentViewPath = Commerce.Navigator.navigationLog()[Commerce.Navigator.navigationLog().length - 1];

            // add classes to element
            // primaryAppBarBackground dynamicsSymbolFont
            var $element: JQuery = $(element);
            $element.addClass("primaryAppBarBackground dynamicsSymbolFont");

            this._element = <Element>$.extend({}, $element[0]); // make a clean copy of the element, (before WinJS has its way with it).
                                                            // uses jquery to actually make a copy not just a reference
            this._options = options;

            this.appBar = new WinJS.UI.AppBar(element, options);
            this.appBar.sticky = true; // Prevents app bar overlapping the original view.

            // extend the app bar object to detect navigation when parent view path changes
            this.appBar.parentViewPath = this._parentViewPath;

            Commerce.AppBarControl.initializeAppBar(this.appBar, appBarAlwaysVisible());
            this.applyWinJsFix($element);

            return this.appBar;
        }

       /**
        * Initializes the MPOS App Bar Control and overwrites the WinJS.UI.AppBar's default show and hide methods
        * when the app bar is configured to be always visible.
        * @param {any} appBar - the app bar being initialized
        * @param {boolean} isAlwaysVisible - specifies whether to overwrite AppBar's default settings
        */
        public static initializeAppBar(appBar: any, isAlwaysVisible: boolean): void {
            if (isAlwaysVisible) {
                ApplicationStorage.setItem(ApplicationStorageIDs.APP_BAR_ALWAYS_VISIBLE, "true"); // update local storage for state persistence
                var $appBar: JQuery = $(appBar.element);
                // show/hide methods animate visibility, to prevent animation managing classes directly.
                $appBar.addClass("hideEllipsis win-appbar-shown").removeClass("win-appbar-hidden");

                // set up our own hide method to prevent hiding
                appBar.hide = function (): void {
                    // this refers to app bar itself
                    var navigationNewView: string = Commerce.Navigator.navigationLog()[Commerce.Navigator.navigationLog().length - 1];
                    if (this.parentViewPath !== navigationNewView) {
                        $appBar.removeClass("win-appbar-shown").addClass("win-appbar-hidden");
                    }
                };

                appBar.show = function (): void {
                    $appBar.addClass("hideEllipsis win-appbar-shown").removeClass("win-appbar-hidden");
                };
            }
        }

       /*
        * Temporary winjs 3.0.1 solution which prevents ellipsis button to submit the form.
        * Used for the pages which contains AppBar and html form element.
        * @param {Jquery} appbar element
        */
        private applyWinJsFix($element: JQuery): void {
            var $form: JQuery = $element.parents("form");
            if ($form.length > 0) {
                $element.find(".win-appbar-invokebutton").each((index: number, elem: HTMLInputElement) => {
                    // jquery doesnt allow changing type for inputs.
                    elem.type = "button";
                });
            }
        }
    }

    // Required for WinJS to process this control as a WinJS control
    WinJS.Utilities.markSupportedForProcessing(AppBarControl);
}