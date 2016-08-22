/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../JQuery.d.ts'/>

module Commerce {
    "use strict";

    export class CSSHelpers {

        public static AXRETAIL_STYLESHEET_ASYNC_REL: string = "-axretail-stylesheet-async";
        public static AXRETAIL_STYLESHEET_ASYNC_LINKS_SELECTOR: string = "link[rel = '" + CSSHelpers.AXRETAIL_STYLESHEET_ASYNC_REL + "']";

        public static LEFT_TO_RIGHT_TEXT_DIRECTION: string = "ltr";
        public static RIGHT_TO_LEFT_TEXT_DIRECTION: string = "rtl";

        public static currentThemeLoaded: Observable<string> = ko.observable(StringExtensions.EMPTY);
        public static deviceConfigurationTheme: string;
        public static currentTextDir: Observable<string> = ko.observable(StringExtensions.EMPTY);

        public static posLightThemeElementId: string = "posLightThemeCSSFile";
        public static posDarkThemeElementId: string = "posDarkThemeCSSFile";
        public static winUILightThemeElementId: string = "winUILightThemeCSSFile";
        public static winUIDarkThemeElementId: string = "winUIDarkThemeCSSFile";
        public static dynamicAccentColorStyleId: string = "dynamicAccentColorStyle";
        public static scrollbarStyleId: string = "scrollbarStyle";

        public static accentColorStringToReplace: string = "%%ACCENT_COLOR%%";
        public static accentColorLight20StringToReplace: string = "%%ACCENT_COLOR_LIGHT_20%%";
        public static accentColorDark20StringToReplace: string = "%%ACCENT_COLOR_DARK_20%%";
        public static logonBackgroundImageStringToReplace: string = "%%LOGIN_BACKGROUND_IMAGE%%";
        public static logonBackgroundImagePortraitStringToReplace: string = "%%LOGIN_BACKGROUND_IMAGE_PORTRAIT%%";
        public static backgroundImageStringToReplace: string = "%%BACKGROUND_IMAGE%%";

        public static accentColor: Observable<string> = ko.observable(StringExtensions.EMPTY);
        public static accentColorId: number;

        // Hardcoded colors for navigation bar and navigation bar extension
        public static navbarDarkTheme: string = "#131415";
        public static navbarExtensionDarkTheme: string = "#333435";

        public static navbarLightTheme: string = "#B3B4B5";
        public static navbarExtensionLightTheme: string = "#D3D4D5";

        /* A root path to resolve full path to css files. By default it is a current document. */
        public static cssFileRootPath: string = StringExtensions.EMPTY;

        private static POS_LIGHT_THEME_CSS_FILE: string = "Stylesheets/Themes/PosLightTheme.css";
        private static POS_DARK_THEME_CSS_FILE: string = "Stylesheets/Themes/PosDarkTheme.css";
        private static WINUI_LIGHT_THEME_CSS_FILE: string = "Libraries/winjs/css/ui-light.css";
        private static WINUI_DARK_THEME_CSS_FILE: string = "Libraries/winjs/css/ui-dark.css";
        private static DYNAMIC_ACCENT_COLOR_CSS_FILE: string = "Stylesheets/Parts/DynamicAccentColor.css";
        private static SCROLLBAR_STYLE_CSS_FILE: string = "Stylesheets/Parts/ScrollbarStyle.min.css";
        private static BLUE_ACCENT_COLOR_KEY: number = 13004559;

        private static accentColors: { [key: number]: string } = {
            13004559: "#0F6FCF", // Blue
            3776084: "#008A00", // Green
            2376177: "#F04325", // Red
            13850523: "#8D4294" // Purple
        };

        private static isDeveloperGridRendered: boolean = false;

        private static navbarExtensionColors: { [key: number]: string } = {
            13004559: "#00477A", // Blue
            3776084: "#005200", // Green
            2376177: "#BC270D", // Red
            13850523: "#642075" // Purple
        };

        /**
         * Apply the theme to the application
         * @param {Proxy.Entities.DeviceConfiguration} [deviceConfiguration] The deviceConfiguration object which contains the theme,
         * accentColor, logon and POS background images
         * @param {Proxy.Entities.DeviceConfiguration} [previousDeviceConfiguration] The previous deviceConfiguration object which is used for comparison.
         * @return {IVoidAsyncResult} The async result.
         */
        public static applyThemeAsync(
            deviceConfiguration?: Proxy.Entities.DeviceConfiguration,
            previousDeviceConfiguration?: Proxy.Entities.DeviceConfiguration): IVoidAsyncResult {
            // set default text direction before app loads
            CSSHelpers.currentTextDir($("body").attr("dir"));

            var theme: string;
            var accentColor: number;
            var logOnBackgroundPictureAsBase64: string;
            var logOnBackgroundPicturePortraitAsBase64: string;
            var backgroundPictureAsBase64: string;

            if (!Commerce.ObjectExtensions.isNullOrUndefined(deviceConfiguration)) {
                accentColor = deviceConfiguration.AccentColor;
                logOnBackgroundPictureAsBase64 = deviceConfiguration.LogOnBackgroundPictureAsBase64;
                logOnBackgroundPicturePortraitAsBase64 = deviceConfiguration.LogOnBackgroundPicturePortraitAsBase64;
                backgroundPictureAsBase64 = deviceConfiguration.BackgroundPictureAsBase64;
                theme = deviceConfiguration.Theme;
            } else {
                theme = "dark";
                accentColor = CSSHelpers.BLUE_ACCENT_COLOR_KEY;
                logOnBackgroundPictureAsBase64 = "";
                logOnBackgroundPicturePortraitAsBase64 = "";
                backgroundPictureAsBase64 = "";
            }

            // Only apply the accent color if the accent color, one of logon background images, or the background image has changed since the previous time.
            // This is an expensive operation, and should be avoided if the values are the same as the previous ones.
            var shouldUpdateAccentColorAndBackgroundImages: boolean = ObjectExtensions.isNullOrUndefined(previousDeviceConfiguration)
                || previousDeviceConfiguration.AccentColor !== accentColor
                || previousDeviceConfiguration.LogOnBackgroundPictureAsBase64 !== logOnBackgroundPictureAsBase64
                || previousDeviceConfiguration.LogOnBackgroundPicturePortraitAsBase64 !== logOnBackgroundPicturePortraitAsBase64
                || previousDeviceConfiguration.BackgroundPictureAsBase64 !== backgroundPictureAsBase64;

            if (shouldUpdateAccentColorAndBackgroundImages) {
                CSSHelpers.applyAccentColor(accentColor,
                    logOnBackgroundPictureAsBase64,
                    logOnBackgroundPicturePortraitAsBase64,
                    backgroundPictureAsBase64);
            }

            CSSHelpers.accentColorId = accentColor;
            CSSHelpers.deviceConfigurationTheme = theme;
            CSSHelpers.applyNavigationBarColors(theme, deviceConfiguration);
            CSSHelpers.applyScrollbarStyle();
            return CSSHelpers.applyThemeColorAsync(theme);
        }

        public static loadAxRetailStylesheetAsync(): IVoidAsyncResult {
            var results: IVoidAsyncResult[] = [];
            $(CSSHelpers.AXRETAIL_STYLESHEET_ASYNC_LINKS_SELECTOR).each((index: any, element: Element) => {
                var $element: JQuery = $(element);
                var uri: string = $element.attr("href");
                var result: IVoidAsyncResult = this.cacheFileContentAsync(uri).done((content: string) => {
                    $element.attr("rel", "stylesheet");
                }).fail((errors: Model.Entities.Error[]) => {
                    RetailLogger.applicationStylesheetsLoadFailed(uri, errors[0].ErrorCode, errors[0].extraData);
                });
                results.push(result);
            });
            return VoidAsyncResult.join(results);
        }

        public static loadCSSDeveloperModePresets(): void {
            CSSHelpers.setDeveloperModeThemeDefault();

            CSSHelpers.generateGridHelper();
            $(window).resize(function (e: JQueryEventObject): void {
                CSSHelpers.isDeveloperGridRendered = false;
                CSSHelpers.generateGridHelper();
            });

            // if dev mode options specify grid is visible, make it visible
            CSSHelpers.setGridHelperVisibility(CSSHelpers.isGridHelperVisible());

            // if dev mode options specify dir, apply dir, otherwise apply default
            CSSHelpers.setDeveloperModeTextDirectionDefault();

            CSSHelpers.setDeveloperModeColoring(CSSHelpers.isDeveloperModeColoringVisible());
        }

        public static setDeveloperModeNavigationLoggingState(isNavigationLoggingEnabled: boolean): void {
            ApplicationStorage.setItem(ApplicationStorageIDs.NAVIGATION_LOGGING_ENABLED, "" + isNavigationLoggingEnabled);
        }

        public static isDeveloperModeNavigationLoggingEnabled(): boolean {
            return !Commerce.ObjectExtensions.isNullOrUndefined(ApplicationStorage.getItem(ApplicationStorageIDs.NAVIGATION_LOGGING_ENABLED)) ?
                ApplicationStorage.getItem(ApplicationStorageIDs.NAVIGATION_LOGGING_ENABLED) === "true" : false;
        }

        public static setDeveloperModeNavigationLogVisibility(isNavigationLogVisible: boolean): void {
            ApplicationStorage.setItem(ApplicationStorageIDs.NAVIGATION_LOG_VISIBLE, "" + isNavigationLogVisible);
        }

        public static isDeveloperModeNavigationLogVisible(): boolean {
            return !Commerce.ObjectExtensions.isNullOrUndefined(ApplicationStorage.getItem(ApplicationStorageIDs.NAVIGATION_LOG_VISIBLE)) ?
                ApplicationStorage.getItem(ApplicationStorageIDs.NAVIGATION_LOG_VISIBLE) === "true" : false;
        }

        public static setCSSDeveloperMode(isDeveloperMode: boolean): void {
            ApplicationStorage.setItem(ApplicationStorageIDs.CSS_DEVMODE, "" + isDeveloperMode);
        }

        public static isCSSDeveloperMode(): boolean {
            if (Commerce.ObjectExtensions.isNullOrUndefined(ApplicationStorage.getItem(ApplicationStorageIDs.CSS_DEVMODE)) ||
                ApplicationStorage.getItem(ApplicationStorageIDs.CSS_DEVMODE) === "undefined") {
                // dev mode doesn't exist in local storage yet, so set it.
                ApplicationStorage.setItem(ApplicationStorageIDs.CSS_DEVMODE, "false");
            }
            return ApplicationStorage.getItem(ApplicationStorageIDs.CSS_DEVMODE) === "true";
        }

        public static setDeveloperModeTheme(theme: string): void {
            if (CSSHelpers.isCSSDeveloperMode()) {
                ApplicationStorage.setItem(ApplicationStorageIDs.CSS_THEME_COLOR, theme);
                CSSHelpers.applyThemeColorAsync(theme);
            } else {
                // restore default if developer mode is switched off
                CSSHelpers.applyThemeColorAsync(CSSHelpers.deviceConfigurationTheme);
            }
        }

        public static setDeveloperModeThemeDefault(): void {
            if (CSSHelpers.isCSSDeveloperMode()) {
                var theme: string = CSSHelpers.getDeveloperModeTheme();
                if (!CSSHelpers.isDeveloperModeThemeSet()) {
                    // no theme has been saved in developer mode options yet,
                    // so populate the current theme (don't set it though...it's already set).
                    ApplicationStorage.setItem(ApplicationStorageIDs.CSS_THEME_COLOR, theme);
                }
                CSSHelpers.applyThemeColorAsync(theme);
            }
        }

        public static getDeveloperModeTheme(): string {
            return CSSHelpers.isDeveloperModeThemeSet() ? ApplicationStorage.getItem(ApplicationStorageIDs.CSS_THEME_COLOR) : CSSHelpers.currentThemeLoaded();
        }

        public static isDeveloperModeThemeSet(): boolean {
            return !Commerce.ObjectExtensions.isNullOrUndefined(ApplicationStorage.getItem(ApplicationStorageIDs.CSS_THEME_COLOR))
                && ApplicationStorage.getItem(ApplicationStorageIDs.CSS_THEME_COLOR) !== "undefined";
        }

        public static getDeveloperModeTextDirection(): string {
            // unsafe
            return CSSHelpers.isDeveloperModeTextDirectionSet()
                ? ApplicationStorage.getItem(ApplicationStorageIDs.CSS_BODY_DIRECTION)
                : ($("body").attr("dir") || CSSHelpers.LEFT_TO_RIGHT_TEXT_DIRECTION);
        }

        /**
         * Gets the information whether text direction is RTL (Right to Left).
         *
         * @returns {boolean} True if text direction is RTL, false otherwise.
         */
        public static isRightToLeft(): boolean {
            return CSSHelpers.currentTextDir() === CSSHelpers.RIGHT_TO_LEFT_TEXT_DIRECTION;
        }

        public static setTextDirection(dir: string): void {
            Commerce.CSSHelpers.currentTextDir(dir);
            $("body").attr("dir", dir);

            if (Commerce.CSSHelpers.isCSSDeveloperMode()) {
                ApplicationStorage.setItem(ApplicationStorageIDs.CSS_BODY_DIRECTION, dir);
            }
        }

        public static setDeveloperModeTextDirectionDefault(): void {
            if (CSSHelpers.isCSSDeveloperMode()) {
                var dir: string = CSSHelpers.getDeveloperModeTextDirection();
                if (!CSSHelpers.isDeveloperModeTextDirectionSet()) {
                    // no direction has been specified in developer options yet,
                    // so populate the current diretion by retrieving the body attribute
                    ApplicationStorage.setItem(ApplicationStorageIDs.CSS_BODY_DIRECTION, dir);
                }
                CSSHelpers.setTextDirection(dir);
            }
        }

        public static isDeveloperModeTextDirectionSet(): boolean {
            return !Commerce.ObjectExtensions.isNullOrUndefined(ApplicationStorage.getItem(ApplicationStorageIDs.CSS_BODY_DIRECTION))
                && ApplicationStorage.getItem(ApplicationStorageIDs.CSS_BODY_DIRECTION) !== "undefined";
        }

        public static setGridHelperVisibility(isVisible: boolean): void {
            if (Commerce.CSSHelpers.isCSSDeveloperMode()) {
                ApplicationStorage.setItem(ApplicationStorageIDs.CSS_DEV_GRID, "" + isVisible);

                if (isVisible) {
                    $("#mposGridlines").show();
                    CSSHelpers.generateGridHelper();
                } else {
                    $("#mposGridlines").hide();
                }
            } else {
                // hide if not in developer mode
                $("#mposGridlines").hide();
            }
        }

        public static isGridHelperVisible(): boolean {
            return !Commerce.ObjectExtensions.isNullOrUndefined(ApplicationStorage.getItem(ApplicationStorageIDs.CSS_DEV_GRID)) ?
                ApplicationStorage.getItem(ApplicationStorageIDs.CSS_DEV_GRID) === "true" : false;
        }

        public static generateGridHelper(): void {
            if (CSSHelpers.isDeveloperGridRendered) {
                return null;
            }
            CSSHelpers.isDeveloperGridRendered = true;
            var gridSize: number = parseInt($("body").css("font-size"), 10);
            var numHorizontalGrids: number = Math.ceil($(window).width() / gridSize);
            var numVerticalGrids: number = Math.ceil($(window).height() / gridSize);
            var cellColor: string = "rgba(255, 0, 0, 0.25)";
            var divisionColor: string = "rgba(0, 255, 255, 0.25)";
            var gridHTML: string = "";
            for (var r: number = 0; r < numVerticalGrids; r++) {
                gridHTML += "<div class='row height1'>";

                var isRowDivision: boolean = r !== 0 && r % 10 === 0;
                var borderTopColor: string = isRowDivision ? divisionColor : cellColor;

                for (var c: number = 0; c < numHorizontalGrids; c++) {
                    var isCellDivision: boolean = c !== 0 && c % 10 === 0;
                    var borderLeftColor: string = isCellDivision ? divisionColor : cellColor;

                    gridHTML += "<div class='width1 height1' style='color: green; border-left: 1px solid " + borderLeftColor + ";";
                    gridHTML += " border-top: 1px solid " + borderTopColor + ";'>";

                    if (isRowDivision && c === 0) {
                        gridHTML += r;
                    }
                    if (isCellDivision && r === 0) {
                        gridHTML += c;
                    }
                    gridHTML += "</div>";
                }
                gridHTML += "</div>";
            }
            $("#mposGridlinesContainer").empty().append(gridHTML);
            $(window).unbind("mousemove").bind("mousemove", function (e: JQueryEventObject): void {
                var positionX: number = e.pageX;
                var positionY: number = e.pageY;
                $("#mposGridCoordsX").html("" + (Math.floor(positionX / 10) * 10 + 10));
                $("#mposGridCoordsY").html("" + (Math.floor(positionY / 10) * 10 + 10));
            });
        }

        public static isDeveloperModeColoringVisible(): boolean {
            return !Commerce.ObjectExtensions.isNullOrUndefined(ApplicationStorage.getItem(ApplicationStorageIDs.CSS_DEV_COLORS)) ?
                ApplicationStorage.getItem(ApplicationStorageIDs.CSS_DEV_COLORS) === "true" : false;
        }

        public static setDeveloperModeColoring(isVisible: boolean): void {
            if (CSSHelpers.isCSSDeveloperMode()) {
                ApplicationStorage.setItem(ApplicationStorageIDs.CSS_DEV_COLORS, "" + isVisible);

                if (isVisible) {
                    CSSHelpers.loadStyleSheetAsync("Stylesheets/Themes/DeveloperMode.css", "developerModeCommonStylesheet");
                    localStorage.setItem("devColors", "true");
                } else {
                    $("#developerModeCommonStylesheet").remove();
                    ApplicationStorage.setItem(ApplicationStorageIDs.CSS_DEV_COLORS, "false");
                }
            } else {
                // hide if not in developer mode
                $("#developerModeCommonStylesheet").remove();
                ApplicationStorage.setItem(ApplicationStorageIDs.CSS_DEV_COLORS, "false");
            }
        }

        /**
         * Converts a color object to CSS rgba style string.
         * @param {ARGBColor} color Color object.
         */
        public static colorToRGBAStyle(color: Commerce.Model.Entities.ARGBColor): string {
            if (Commerce.ObjectExtensions.isNullOrUndefined(color)) {
                color = { A: 0, R: 0, G: 0, B: 0 };
            }

            return "rgba(" + color.R + "," +
                color.G + "," +
                color.B + "," +
                (color.A / 255) + ")";
        }

        /**
         * Converts a color int to CSS rgba style string.
         * @param {number} color Color value.
         */
        public static colorIntToRGBAStyle(color: number): string {
            var rgbaColor: string = "";
            if (color !== 0) {
                
                color >>>= 0;
                var r: number = color & 0xFF,
                    g: number = (color & 0xFF00) >>> 8,
                    b: number = (color & 0xFF0000) >>> 16,
                
                    a: number = 1; // opacity is always 1
                rgbaColor = "rgba(" + [r, g, b, a].join(",") + ")";
            } else {
                // Sets the default color for the application until we get information from the server
                // Color:#00B0F0
                rgbaColor = "rgba(0, 176, 240, 1)";
            }
            return rgbaColor;
        }

        /**
         * Sets a CSS style for a DOM element. This function is necessary to apply
         * non-standard CSS styles like "-ms-grid-column".
         * @param {JQuery} $element The DOM element to set CSS style for.
         * @param {string} styleName The CSS style name.
         * @param {string} styleValue The CSS style valuT.
         */
        public static setStyle($element: JQuery, styleName: string, styleValue: string): void {
            if (Commerce.ObjectExtensions.isNullOrUndefined($element) ||
                Commerce.ObjectExtensions.isNullOrUndefined(styleName) ||
                Commerce.ObjectExtensions.isNullOrUndefined(styleValue)) {
                return;
            }

            styleName = EscapingHelper.escapeHtmlAttribute(styleName.toLocaleLowerCase());
            styleValue = EscapingHelper.escapeHtmlAttribute(styleValue.toLocaleLowerCase());

            var currentStyleAttr: string = $element.attr("style");
            if (Commerce.ObjectExtensions.isNullOrUndefined(currentStyleAttr)) {
                currentStyleAttr = "";
            }

            var currentStyles: string[] = currentStyleAttr.split(";");
            var styleFound: boolean = false;
            for (var i: number = 0; i < currentStyles.length; i++) {
                var style: string = currentStyles[i];
                if (style.split(":")[0].trim().toLocaleLowerCase().localeCompare(styleName) === 0) {
                    currentStyles[i] = styleName + ":" + styleValue;
                    styleFound = true;
                    break;
                }
            }

            if (!styleFound) {
                currentStyles.push(styleName + ":" + styleValue);
            }

            $element.attr("style", currentStyles.join(";"));
        }

        /**
         * Apply the accent color to application
         * @param {number} accentColor The accent color.
         * @param {string} logonBackgroundImage The log on background image.
         * @param {string} logonBackgroundImagePortrait The log on background portrait image.
         * @param {number} backgroundImage The background image.
         */
        public static applyAccentColor(accentColor: number,
            logonBackgroundImage: string = "",
            logonBackgroundImagePortrait: string = "",
            backgroundImage: string = ""): void {

            // Mapping for css correction
            var mapDictionary: Dictionary<string> = new Dictionary<string>();
            mapDictionary.setItem(CSSHelpers.logonBackgroundImageStringToReplace, logonBackgroundImage);
            mapDictionary.setItem(CSSHelpers.logonBackgroundImagePortraitStringToReplace, logonBackgroundImagePortrait);
            mapDictionary.setItem(CSSHelpers.backgroundImageStringToReplace, backgroundImage);

            if (!ObjectExtensions.isNullOrUndefined(accentColor)) {
                var accentColorConverted: string = CSSHelpers.accentColors[accentColor];
                if (!ObjectExtensions.isNullOrUndefined(accentColorConverted)) {
                    CSSHelpers.accentColor(accentColorConverted);
                    mapDictionary.setItem(CSSHelpers.accentColorStringToReplace, accentColorConverted);
                    var accentColorLight20: string = CSSHelpers.shadeColor(accentColorConverted, 0.2);
                    mapDictionary.setItem(CSSHelpers.accentColorLight20StringToReplace, accentColorLight20);
                    var accentColorDark20: string = CSSHelpers.shadeColor(accentColorConverted, -0.2);
                    mapDictionary.setItem(CSSHelpers.accentColorDark20StringToReplace, accentColorDark20);
                }
            }

            this.cacheFileContentAsync(this.dynamicAccentColorCssFile).done((content: string) => {
                var inlinedStyleElement: HTMLElement = document.getElementById(CSSHelpers.dynamicAccentColorStyleId);
                if (ObjectExtensions.isNullOrUndefined(inlinedStyleElement)) {
                    inlinedStyleElement = document.createElement("style");
                    inlinedStyleElement.id = CSSHelpers.dynamicAccentColorStyleId;
                    document.head.insertBefore(inlinedStyleElement, null);
                }
                inlinedStyleElement.innerHTML = CSSHelpers.updateDynamicAccentColorCSSRules(content, mapDictionary);
            });
        }

        /**
         * Applies style for scroll bar to avoid misalignment issue in Chrome on PC
         */
        public static applyScrollbarStyle(): void {
            var isAndroid: any = navigator.userAgent.match(/Android/i);
            var isIpad: any = navigator.userAgent.match(/iPad/i);

            // Apply scroll bar style only for PC
            if (ObjectExtensions.isNullOrUndefined(isAndroid) && ObjectExtensions.isNullOrUndefined(isIpad)) {
                this.cacheFileContentAsync(this.scrollbarStyleCssFile).done((content: string) => {
                    var scrollbarStyleElement: HTMLElement = document.getElementById(CSSHelpers.scrollbarStyleId);
                    if (ObjectExtensions.isNullOrUndefined(scrollbarStyleElement)) {
                        scrollbarStyleElement = document.createElement("style");
                        scrollbarStyleElement.id = CSSHelpers.scrollbarStyleId;
                        document.head.insertBefore(scrollbarStyleElement, null);
                    }
                    scrollbarStyleElement.innerHTML = content;
                });
            }
        }

        /**
         * Applies the navigation bar and navigation bar extension color
         * 
         * @param {Model.Entities.DeviceConfiguration} deviceConfiguration The deviceConfiguration object which contains \
         * the theme, accentColor, logon and POS background images
         */
        private static applyNavigationBarColors(theme: string, deviceConfiguration: Model.Entities.DeviceConfiguration): void {
            var navigationBarColor: string = CSSHelpers.navbarDarkTheme;
            var navigationBarExtensionBackgroundColor: string = CSSHelpers.navbarExtensionDarkTheme;

            if (deviceConfiguration && !ObjectExtensions.isNullOrUndefined(deviceConfiguration.AccentColor)) {
                var deviceAccentColor: number = deviceConfiguration.AccentColor;
                navigationBarColor = CSSHelpers.accentColors[deviceAccentColor] || navigationBarColor;
                navigationBarExtensionBackgroundColor = CSSHelpers.navbarExtensionColors[deviceAccentColor]
                                                                    || navigationBarExtensionBackgroundColor;

            } else {
                switch (theme) {
                    case "light":
                        navigationBarColor = CSSHelpers.navbarLightTheme;
                        navigationBarExtensionBackgroundColor = CSSHelpers.navbarExtensionLightTheme;
                        break;
                }
            }

            var navBarColorRule: string = StringExtensions.format("background-color: {0} !important",
                                                                    navigationBarColor);
            var navBarExtensionColorRule: string = StringExtensions.format("background-color: {0} !important",
                                                                            navigationBarExtensionBackgroundColor);

            var styleSheet: StyleSheet = CSSHelpers.findStyleSheet("Main.min.css$");

            CSSHelpers.addCssRuleToStyleSheet(styleSheet, ".navBarColor", navBarColorRule);
            CSSHelpers.addCssRuleToStyleSheet(styleSheet, ".navBarExtensionBackGround", navBarExtensionColorRule);
        }

        /**
         * Applies the given theme to the application
         * 
         * @param {string} theme The theme name to be applied.
         */
        private static applyThemeColorAsync(theme: string): IVoidAsyncResult {
            if (CSSHelpers.currentThemeLoaded() !== theme) { // ensures theme is not already loaded
                switch (theme) {
                    case "light":
                        CSSHelpers.removeOppositeThemeStyleSheets(CSSHelpers.posDarkThemeElementId, CSSHelpers.winUIDarkThemeElementId);
                        return CSSHelpers.loadCurrentThemeStyleSheetsAsync(theme,
                            CSSHelpers.posLightThemeCssFile,
                            CSSHelpers.winUILightThemeCssFile,
                            CSSHelpers.posLightThemeElementId,
                            CSSHelpers.winUILightThemeElementId);
                    default:
                        CSSHelpers.removeOppositeThemeStyleSheets(CSSHelpers.posLightThemeElementId, CSSHelpers.winUILightThemeElementId);
                        return CSSHelpers.loadCurrentThemeStyleSheetsAsync(theme,
                            CSSHelpers.posDarkThemeCssFile,
                            CSSHelpers.winUIDarkThemeCssFile,
                            CSSHelpers.posDarkThemeElementId,
                            CSSHelpers.winUIDarkThemeElementId);
                }
            } else {
                return VoidAsyncResult.createResolved();
            }
        }

        private static loadCurrentThemeStyleSheetsAsync(theme: string,
            posThemeCssFile: string,
            winUIThemeCssFile: string,
            posThemeElementId: string,
            winUIThemeElementId: string): IVoidAsyncResult {

            return new AsyncQueue()
                .enqueue(() => CSSHelpers.loadStyleSheetAsync(Commerce.StringExtensions.format(posThemeCssFile, theme), posThemeElementId))
                .enqueue(() => CSSHelpers.loadStyleSheetAsync(Commerce.StringExtensions.format(winUIThemeCssFile, theme), winUIThemeElementId))
                .run()
                .done(() => Commerce.CSSHelpers.currentThemeLoaded(theme));
        }

        private static removeOppositeThemeStyleSheets(oppositePosThemeElementId: string, oppositeWinUIThemeElementId: string): void {
            if (document.getElementById(oppositePosThemeElementId)) {
                CSSHelpers.removeStyleSheetById(oppositePosThemeElementId);
            }

            if (document.getElementById(oppositeWinUIThemeElementId)) {
                CSSHelpers.removeStyleSheetById(oppositeWinUIThemeElementId);
            }
        }

        private static cacheFileContentAsync(uri: string): IAsyncResult<string> {
            var result: AsyncResult<string> = new AsyncResult<string>();
            $.ajax({
                url: uri,
                success: (data: any, textStatus: string, jqXHR: JQueryXHR): void => {
                    result.resolve(data);
                },
                error: (jqXHR: JQueryXHR, textStatus: string, errorThrown: string): void => {
                    result.reject([new Model.Entities.Error(textStatus, false, null, errorThrown)]);
                },
                cache: true
            });
            return result;
        }

        /**
         * Updates content of DynamicAccentColor.css file: replaces placeholders by specific values. 
         * @param {string} cssFileContent Content of CSS file.
         * @param {string} mapping Represents mapping for placeholders and values by which placeholders should be replaced.
         */
        private static updateDynamicAccentColorCSSRules(cssFileContent: string, mapping: Dictionary<string>): string {
            cssFileContent = cssFileContent.replace(cssFileContent.substring(cssFileContent.indexOf("/*"),
                                                    cssFileContent.indexOf("*/") + 2),
                                                    StringExtensions.EMPTY);
            mapping.forEach((key: string, value: string) => {
                cssFileContent = StringExtensions.replaceAll(cssFileContent, key, value);
            });

            return cssFileContent;
        }

        /**
         * Loads a css file by the given filename
         * @param {string} filename The CSS filename to be loaded
         * @param {string} id A unique element id which can be used to access the element
         */
        private static loadStyleSheetAsync(filename: string, id: string): IVoidAsyncResult {
            var cachedCallback: () => void;
            cachedCallback = () => {
                var link: HTMLLinkElement = document.createElement("link");
                link.setAttribute("rel", "stylesheet");
                link.setAttribute("type", "text/css");
                link.setAttribute("href", filename);
                link.setAttribute("id", id);
                var headElement: HTMLHeadElement = document.getElementsByTagName("head")[0];
                headElement.insertBefore(link, headElement.childNodes[0]);
            };
            var result: VoidAsyncResult = new VoidAsyncResult();
            this.cacheFileContentAsync(filename)
                        .always(() => {
                            cachedCallback();
                            result.resolve();
                        })
                        .fail((errors: Model.Entities.Error[]) => {
                            RetailLogger.applicationStylesheetsLoadFailed(filename, errors[0].ErrorCode, errors[0].extraData);
                        });
            return result;
        }

        /**
         * Removes a css file by the given id
         * 
         * @param {string} id The id of the <link> or css file tag to be removed from the head
         */
        private static removeStyleSheetById(id: string): void {
            $("head link[id=" + id + "]").remove();
        }

        /**
         * Gets the css file.
         * @param {any} regex Regular expression.
         */
        private static findStyleSheet(regex: string): StyleSheet {
            for (var i: number = 0; i < document.styleSheets.length; i++) {
                if (document.styleSheets[i].href && document.styleSheets[i].href.match(regex)) {
                    return document.styleSheets[i];
                }
            }

            return null;
        }

        /**
         * Add css rule to css.
         * @param {any} styleSheet The CSS file.
         * @param {string} selector rule selector.
         * @param {string} rule CSS rule.
         */
        private static addCssRuleToStyleSheet(styleSheet: any, selector: string, rule: string): void {
            if (!styleSheet) {
                return;
            }

            // Remove the rule from stylesheet.
            CSSHelpers.removeCssRulesFromStyleSheet(styleSheet, selector);

            if (styleSheet.addRule) {
                styleSheet.addRule(selector, rule);
            } else if (styleSheet.insertRule) {
                styleSheet.insertRule(Commerce.StringExtensions.format("{0} \{ {1} \}", selector, rule));
            }
        }

        /**
         * Remove css rule from css.
         * @param {any} styleSheet The CSS file.
         * @param {string} selector CSS rule.
         */
        private static removeCssRulesFromStyleSheet(styleSheet: any, selector: string): void {
            if (!styleSheet || !selector) {
                return;
            }

            if (styleSheet.rules) {
                var i: number = styleSheet.rules.length - 1;
                while (i > -1) {
                    var rule: any = styleSheet.rules[i];
                    if (rule.selectorText && rule.selectorText.localeCompare(selector) === 0) {
                        if (styleSheet.removeRule) {
                            styleSheet.removeRule(i);
                        } else if (styleSheet.deleteRule) {
                            styleSheet.deleteRule(i);
                        }
                    }
                    i--;
                }
            }
        }

        private static resolveCssFilePath(cssFile: string): string {
            return CSSHelpers.cssFileRootPath + cssFile;
        }

        /**
         * Converts color to a specific shade.
         * @param {string} color The color.
         * @param {number} shade The shade (-1.0 to 1.0).
         */
        private static shadeColor(color: string, shade: number): string {
            if (color[0] === "#") {
                color = color.slice(1);
            }

            var colorNumber: number = parseInt(color, 16);
            var edge: number = shade < 0 ? 0 : 255;
            var p: number = shade < 0 ? shade * -1 : shade;

            

            var R: number = colorNumber >> 16;
            var G: number = colorNumber >> 8 & 0x00FF;
            var B: number = colorNumber & 0x0000FF;

            

            var newR: number = (Math.round((edge - R) * p) + R) * 0x10000;
            var newG: number = (Math.round((edge - G) * p) + G) * 0x100;
            var newB: number = Math.round((edge - B) * p) + B;

            return "#" + (0x1000000 + newR + newG + newB).toString(16).slice(1);
        }

        private static get posLightThemeCssFile(): string {
            return CSSHelpers.resolveCssFilePath(CSSHelpers.POS_LIGHT_THEME_CSS_FILE);
        }

        private static get posDarkThemeCssFile(): string {
            return CSSHelpers.resolveCssFilePath(CSSHelpers.POS_DARK_THEME_CSS_FILE);
        }

        private static get winUILightThemeCssFile(): string {
            return CSSHelpers.resolveCssFilePath(CSSHelpers.WINUI_LIGHT_THEME_CSS_FILE);
        }

        private static get winUIDarkThemeCssFile(): string {
            return CSSHelpers.resolveCssFilePath(CSSHelpers.WINUI_DARK_THEME_CSS_FILE);
        }

        private static get dynamicAccentColorCssFile(): string {
            return CSSHelpers.resolveCssFilePath(CSSHelpers.DYNAMIC_ACCENT_COLOR_CSS_FILE);
        }

        private static get scrollbarStyleCssFile(): string {
            return CSSHelpers.resolveCssFilePath(CSSHelpers.SCROLLBAR_STYLE_CSS_FILE);
        }
    }
}
