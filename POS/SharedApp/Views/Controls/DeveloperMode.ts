/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='UserControl.ts'/>

module Commerce.Controls {
    "use strict";

    export class DeveloperMode extends UserControl {


        public static darkTheme: string = "dark";
        public static lightTheme: string = "light";
        public static leftDirection: string = "rtl";
        public static rightDirection: string = "ltr";

        public static isVisible: boolean;
        public static theme: Observable<string>;
        public static dir: Observable<string>;
        public static isEventAttached: boolean = false;
        public static isColoringVisible: boolean;

        private _isColoringDisabled: Computed<boolean>;
        private _isDeveloperMode: Observable<boolean>;
        private _isGridToggleDisabled: Computed<boolean>;

        constructor() {
            super();
            this._isDeveloperMode = ko.observable(Commerce.CSSHelpers.isCSSDeveloperMode());
            this._isGridToggleDisabled = ko.computed(() => { return !this._isDeveloperMode(); }, this);
            this._isColoringDisabled = ko.computed(() => { return !this._isDeveloperMode(); }, this);


            this._isDeveloperMode.subscribe((newValue: boolean) => {
                CSSHelpers.setCSSDeveloperMode(newValue);

                if (CSSHelpers.isCSSDeveloperMode()) {
                    // apply default selected values for theme, dir, and grid
                    CSSHelpers.setDeveloperModeTheme(DeveloperMode.theme());
                    CSSHelpers.setTextDirection(DeveloperMode.dir());
                    CSSHelpers.setGridHelperVisibility(DeveloperMode.isVisible);
                    CSSHelpers.setDeveloperModeColoring(DeveloperMode.isColoringVisible);

                    if (DeveloperMode.isEventAttached === false) {
                        $(document).on("keydown", DeveloperMode.keyDown);
                        DeveloperMode.isEventAttached = true;
                    }

                } else {
                    // otherwise, remove presets and show defaults
                    CSSHelpers.setTextDirection(CSSHelpers.currentTextDir());
                    CSSHelpers.setGridHelperVisibility(false);
                    CSSHelpers.setDeveloperModeTheme(CSSHelpers.currentThemeLoaded());
                    CSSHelpers.setDeveloperModeColoring(false);

                    if (DeveloperMode.isEventAttached) {
                        $(document).off("keydown", DeveloperMode.keyDown);
                        DeveloperMode.isEventAttached = false;
                    }
                }

            }, this);

            DeveloperMode.isVisible = CSSHelpers.isGridHelperVisible();
            DeveloperMode.isColoringVisible = CSSHelpers.isDeveloperModeColoringVisible();

            DeveloperMode.theme = ko.observable(CSSHelpers.getDeveloperModeTheme());
            DeveloperMode.theme.subscribe((newValue: string) => {
                DeveloperMode.theme(newValue);
                CSSHelpers.setDeveloperModeTheme(newValue);
            }, this);

            DeveloperMode.dir = ko.observable(CSSHelpers.getDeveloperModeTextDirection());
            DeveloperMode.dir.subscribe((newValue: string) => {
                DeveloperMode.dir(newValue);
                CSSHelpers.setTextDirection(newValue);
            }, this);

            if (this._isDeveloperMode() && !DeveloperMode.isEventAttached) {
                $(document).on("keydown", DeveloperMode.keyDown);
                    DeveloperMode.isEventAttached = true;
            }
        }

        public static toggleGridVisibility(): void {
            DeveloperMode.isVisible = !DeveloperMode.isVisible;
            Commerce.CSSHelpers.setGridHelperVisibility(DeveloperMode.isVisible);
        }

        public static toggleColorVisibility(): void {
            DeveloperMode.isColoringVisible = !DeveloperMode.isColoringVisible;
            Commerce.CSSHelpers.setDeveloperModeColoring(DeveloperMode.isColoringVisible);
        }

        private static keyDown(event: JQueryEventObject): void {
            if (event.ctrlKey && event.altKey) {
                switch (String.fromCharCode(event.which)) {
                    case "G":
                        DeveloperMode.toggleGridVisibility();
                        break;
                    case "T":
                        DeveloperMode.theme(DeveloperMode.theme() === DeveloperMode.darkTheme
                            ? DeveloperMode.lightTheme : DeveloperMode.darkTheme);
                        break;
                    case "D":
                        DeveloperMode.dir(DeveloperMode.dir() === DeveloperMode.leftDirection
                            ? DeveloperMode.rightDirection : DeveloperMode.leftDirection);
                        break;
                    case "C":
                        DeveloperMode.toggleColorVisibility();
                        break;
                }
            }
        }
    }
}