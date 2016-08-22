/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Utilities/CSSHelpers.ts' />
///<reference path='../Utilities/Dictionary.ts'/>
///<reference path='../Core.d.ts'/>
///<reference path='CommerceTypes.g.ts'/>

module Commerce.Proxy.Entities {
    /**
     * Layout container.
     */
    export class Layout {
        public ID: string = "";
        public Type: string = "";
        public Title: string = "";
        public TabStripPlacement: string = "";
        public Top: number;
        public Left: number;
        public Height: number;
        public Width: number;
        public ImageID: number;
        public ZoneID: string = "";
        public DisplayTitleAboveControl: boolean;
        public HideButtons: boolean;
        public IsBrowseBarAlwaysVisible: boolean;
        public Content: Layout;
        public DesignerControls: Layout[];
        public TabPages: Layout[];
        public SelectedDeliveryFields: Layout[];
        public SelectedLinesFields: Layout[];
        public SelectedPaymentsFields: Layout[];
        public LeftSelectedTotalsFields: Layout[];
        public RightSelectedTotalsFields: Layout[];
        public StretchImageToFit: boolean;
    }

    /**
     * Custom control defined in till layout.
     */
    export interface CustomControl extends Layout {
        RelativeUri: string;
    }

    /**
     * Screen layout container. Contains layout for both orientations.
     */
    export class ScreenLayout {
        public Landscape: Layout;
        public Portrait: Layout;
    }

    /*
     * Default start screen enum.
     */
    export enum StartScreen {
        Welcome = 0,
        Transaction = 1
    }

    /**
     * Screen layout container. Contains layout for both orientations.
     */
    export class Orientation {
        public static Landscape = "Landscape";
        public static Portrait = "Portrait";
    }

    /**
     * TillLayout proxy.
     */
    export class TillLayoutProxy {
        private _buttonGridsByZoneId: any = null;
        private _buttonGridsById: any = null;
        private _allButtonGrids: Entities.ButtonGrid[];
        private _tillLayoutResponse: Model.Entities.TillLayout;
        private _layoutCssPrefix = " .tillLayout-";
        public transactionScreenLayout: ScreenLayout;
        public orientation: string;
        private _landscapeMediaQuery: MediaQueryList;
        private _orientationSwitchHandler: any;
        private _orientationChangedEventName = "OrientationChangedEvent";
        private _startView = "HomeView";

        constructor(tillLayoutResponse: Model.Entities.TillLayout) {
            this._tillLayoutResponse = tillLayoutResponse;
            this.transactionScreenLayout = new ScreenLayout();
            var tillLayoutResponseDefined: boolean = !ObjectExtensions.isNullOrUndefined(tillLayoutResponse);

            // If start screen other than welcome view.
            // WelcomeScreen = 0, TransactionScreen = 1
            if (tillLayoutResponseDefined && ObjectExtensions.isNumber(this._tillLayoutResponse.StartScreen) && this._tillLayoutResponse.StartScreen === StartScreen.Transaction) {
                this._startView = "CartView";
            }

            if (!tillLayoutResponseDefined || StringExtensions.isNullOrWhitespace(tillLayoutResponse.LayoutXml)) {
                Commerce.ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString("string_4"), MessageType.Info, MessageBoxButtons.Default, Commerce.ViewModelAdapter.getResourceString("string_1"));
            }
            else {
                try {
                    // Convert layout json into object
                    this.transactionScreenLayout.Landscape = JSON.parse(tillLayoutResponse.LayoutXml);
                    if (tillLayoutResponse.LayoutXmlPortrait) {
                        this.transactionScreenLayout.Portrait = JSON.parse(tillLayoutResponse.LayoutXmlPortrait);
                    }
                } catch (ex) {
                    Commerce.ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString("string_5"), MessageType.Info, MessageBoxButtons.Default, Commerce.ViewModelAdapter.getResourceString("string_1"));
                    return;
                }

                // Build css rules for TillLayoutElements
                var cssContent: string[] = [];
                var classPrefix = this._layoutCssPrefix + 'TransactionScreenLayout';

                // To ensure element show/hide media quary wrapper must be applied to both orientations.
                cssContent[cssContent.length] = "@media (orientation:landscape) {";
                this.buildCss(cssContent, this.transactionScreenLayout.Landscape.DesignerControls, classPrefix);
                cssContent[cssContent.length] = "}";

                if (this.transactionScreenLayout.Portrait) {
                    cssContent[cssContent.length] = "@media (orientation:portrait) {";
                    this.buildCss(cssContent, this.transactionScreenLayout.Portrait.DesignerControls, classPrefix);
                    cssContent[cssContent.length] = "}";
                }

                // Find element 
                var $css = $('head').find('style[title=TillLayoutStyles]');
                $css.text(cssContent.join(''));
            }

            var self = this; // window.matchMedia does not trigger handler in correct context.
            this._orientationSwitchHandler = (mediaQueryArgs) => {
                self.orientation = mediaQueryArgs.matches ? Orientation.Landscape : Orientation.Portrait;
                Commerce.ViewModelAdapter.raiseViewEvent(self._orientationChangedEventName, self.orientation);
            };

            this.attachOrientationTracking();
        }

        /**
         * Gets the start view name.
         */
        public get startView(): string {
            return this._startView;
        }

        /**
         * FontScheme enum
         */
        private fontSchemeFormatter(fontScheme: number): string {
            return fontScheme === 0 ? "10px" : "15px";
        }

        /**
         * Creates media query and adds event handler.
         */
        private attachOrientationTracking() {
            if (!this._landscapeMediaQuery) {
                // Find media query matches to detect orientation.
                this._landscapeMediaQuery = window.matchMedia("(orientation: landscape)");
                // Init orientation value.
                this.orientation = this._landscapeMediaQuery.matches ? Orientation.Landscape : Orientation.Portrait;

                // Add a media query change listener
                this._landscapeMediaQuery.addListener(this._orientationSwitchHandler);
            }
        }

        /**
         * Removes all orientation changed handlers.
         */
        public clearOrientationChangedHandlers(): void {
            if (this._landscapeMediaQuery) {
                this._landscapeMediaQuery.removeListener(this._orientationSwitchHandler);
                this._landscapeMediaQuery = null;
            }
        }

        /**
         * Adds orientation changed handler.
         *
         * @param {Element} element DOM element that will need to handle event.
         * @param {(eventArgs: string) => void} eventHandler The event handler.
         */
        public addOrientationChangedHandler(element: Element, eventHandler: (eventArgs: string) => void) {
            if (eventHandler) {
                ViewModelAdapter.addViewEvent(element, this._orientationChangedEventName, eventHandler);
            }
        }

        /**
         * Removes orientation changed handler.
         *
         * @param {Element} element DOM element that will need to handle event.
         * @param {(eventArgs: string) => void} eventHandler The event handler.
         */
        public removeOrientationChangedHandler(element: Element, eventHandler: (eventArgs: string) => void) {
            if (eventHandler) {
                ViewModelAdapter.removeViewEvent(element, this._orientationChangedEventName, eventHandler);
            }
        }

        /**
         * Build css rules for all control that can be customized using css.
         */
        private buildCss(cssContent: string[], designerControls: Layout[], prefix: string): void {
            if (ArrayExtensions.hasElements(designerControls)) {
                designerControls.forEach((designerControl, index, array) => {

                    // add control id prefix to selector
                    var controlClassPrefix = prefix + " #" + designerControl.ID.replace(' ', '_');

                    switch (designerControl.Type) {
                        case "Microsoft.Dynamics.Retail.TillLayoutDesigner.Contracts.Controls.ITransactionGrid":
                            this.buildITransactionGridCss(cssContent, designerControl, controlClassPrefix);
                            break;
                        case "Microsoft.Dynamics.Retail.TillLayoutDesigner.Contracts.Controls.ITabControl":
                            // Search TabControl tabs in case some controls were moved there.
                            this.buildCss(cssContent, designerControl.TabPages, prefix);
                            break;
                        case "Microsoft.Dynamics.Retail.TillLayoutDesigner.Contracts.Controls.ILogo":
                            // Search TabControl tabs in case some controls were moved there.
                            this.buildILogoCss(cssContent, designerControl, controlClassPrefix);
                            break;
                        case "Microsoft.Dynamics.Retail.TillLayoutDesigner.Contracts.Controls.ICustomerPanel":
                            this.buildCustomerPanelCss(cssContent, designerControl, controlClassPrefix);
                            break;
                        case "Microsoft.Dynamics.Retail.TillLayoutDesigner.Contracts.Controls.IButtonGrid":
                            this.buildButtonGridCss(cssContent, designerControl, controlClassPrefix);
                            break;
                    }

                    // Tab content can have control that need to be parsed.
                    if (designerControl.Content && designerControl.Content.Type) {
                        this.buildCss(cssContent, [designerControl.Content], prefix);
                    }

                    this.buildControlDimensionsCss(cssContent, designerControl, controlClassPrefix);

                });
            }
        }

        /**
         * Build ILogo control css.
         *
         * @param {string[]} cssContent The list of css rules.
         * @param {Layout} designerControl The designer control.
         * @param {string} prefix The css class name.
         */
        private buildILogoCss(cssContent: string[], designerControl: Layout, prefix: string) {
            if (!ObjectExtensions.isNullOrUndefined(designerControl.ImageID)) {
                var imageZones = this.getImageZones([designerControl.ZoneID]);

                if (imageZones.length > 0) {
                    cssContent.push(prefix + " { background-repeat: no-repeat;background-position-y: center;");
                    cssContent.push("background-image: url('data:image;base64," + imageZones[0].PictureAsBase64 + "');");

                    if (designerControl.StretchImageToFit) {
                        cssContent.push("background-size: cover;");
                    }

                    cssContent.push("} ");
                }
            }
        }

        /**
         * Build customer panel css.
         * @param {string[]} cssContent The list of css rules.
         * @param {Layout} designerControl The designer control.
         * @param {string} cssClass The css class name.
         */
        private buildCustomerPanelCss(cssContent: string[], designerControl: Layout, cssClass: string): void {

            var layoutDataWithDisplayBlock = {
                "ShowImage": designerControl["ShowImage"],
                "ShowName": designerControl["ShowName"],
                "ShowAccountNumber": designerControl["ShowAccountNumber"],
            };

            for (var key in layoutDataWithDisplayBlock) {
                var fieldCssClass = cssClass + " ." + key;
                if (!layoutDataWithDisplayBlock[key] || layoutDataWithDisplayBlock[key] == 0) {
                    cssContent.push(fieldCssClass + " { display: none; }");
                } else {
                    cssContent.push(fieldCssClass + " { display: block; }");
                }
            }

            var layoutDataWithDisplayFlex = {
                "ShowLoyaltyCard": designerControl["ShowLoyaltyCard"],
                "ShowBalance": designerControl["ShowBalance"],
                "ShowCreditLimit": designerControl["ShowCreditLimit"]
            };

            for (var key in layoutDataWithDisplayFlex) {
                var fieldCssClass = cssClass + " ." + key;
                if (!layoutDataWithDisplayFlex[key] || layoutDataWithDisplayFlex[key] == 0) {
                    cssContent.push(fieldCssClass + " { display: none; }");
                } else {
                    cssContent.push(fieldCssClass + " { display: flex; }");
                }
            }

            // If 1 display primary address
            // If 2 display invoice address
            var addressType = designerControl["AddressType"];
            if (addressType == 1) {
                cssContent.push(cssClass + " .customerPanelPrimaryAddress { display: flex; }");
                cssContent.push(cssClass + " .customerPanelInvoiceAddress { display: none; }");
            }
            else if (addressType == 2) {
                cssContent.push(cssClass + " .customerPanelInvoiceAddress { display: flex; }");
                cssContent.push(cssClass + " .customerPanelPrimaryAddress { display: none; }");
            }
        }

        /**
         * Build control dimensions css.
         *
         * @param {string[]} cssContent The list of css rules.
         * @param {Layout} designerControl The designer control.
         * @param {string} cssClass The css class name.
         */
        private buildControlDimensionsCss(cssContent: string[], designerControl: Layout, cssClass: string): void {
            cssContent.push(cssClass + "{ ");

            var addCssProperty = (propertyName: string) => {
                var controlProperty = designerControl[propertyName];
                if (!ObjectExtensions.isNullOrUndefined(controlProperty)) {
                    cssContent.push(propertyName.toLowerCase() + ": " + controlProperty + "px; ");
                }
            };

            cssContent.push("display: block;"); // Show element first
            addCssProperty("Left");
            addCssProperty("Top");
            addCssProperty("Width");
            addCssProperty("Height");
            cssContent.push("} ");
        }

        /**
         * Build css rules for cart view grids.
         */
        private buildITransactionGridCss(cssContent: string[], designerControl: Layout, prefix: string): void {

            // Apply font-size setting from AX to the transaction grid
            var transactionGridListviewPrefix = prefix + " .listViewContainer";
            var fontSize = this.fontSchemeFormatter(Commerce.ApplicationContext.Instance.deviceConfiguration.FontScheme);
            cssContent.push(transactionGridListviewPrefix + " { font-size: " + fontSize + "; }");

            var buildGridCss = (propertyName: string) => {
                var controls = designerControl[propertyName];
                var gridPrefix = prefix + this._layoutCssPrefix + propertyName;

                if (ArrayExtensions.hasElements(controls)) {
                    var cartLinesColumns = [];

                    controls.forEach((control: Layout, index, array) => {
                        if ((!ObjectExtensions.isNullOrUndefined(control.Width)) && (control.Width > 0)) {
                            cartLinesColumns.push(StringExtensions.format('{0} >{1}{2}{ min-width:{3}px; max-width:{3}px; } ', gridPrefix, this._layoutCssPrefix, control.ID, control.Width));
                        }

                        // construct: .prefix .gridName > .field { order: 1; } Actual order value will vary
                        cartLinesColumns.push(StringExtensions.format('{0} >{1}{2}{ order:{3}; display: block; flex-grow:1; flex-basis:0; } ', gridPrefix, this._layoutCssPrefix, control.ID, index + 1));

                    });

                    // construct: .prefix .gridName > * { display:none; }
                    cssContent.push(StringExtensions.format('{0} > * { display:none; } ', gridPrefix)); // hide all fields by default.
                    cssContent.push(cartLinesColumns.join(''));
                }
            };

            buildGridCss("SelectedLinesFields");
            buildGridCss("SelectedPaymentsFields");
            buildGridCss("SelectedDeliveryFields");
        }

        /**
         * Build customer panel css.
         * @param {string[]} cssContent The list of css rules.
         * @param {Layout} designerControl The designer control.
         * @param {string} cssClass The css class name.
         */
        private buildButtonGridCss(cssContent: string[], designerControl: Layout, cssClass: string): void {
            var displayTitleAboveControl = designerControl["DisplayTitleAboveControl"];
            var fieldCssClass = cssClass + " .titleAboveControl";
            if (displayTitleAboveControl) {
                cssContent.push(fieldCssClass + " { display: block !important; }");
            } else {
                cssContent.push(fieldCssClass + " { display: none !important; }");
            }
        }

        /**
         * Gets all button grid zones.
         */
        public getButtonGridZones() {
            if (this._tillLayoutResponse) {
                return this._tillLayoutResponse.ButtonGridZones;
            }

            return null;
        }

        /**
         * Gets a array of image zones.
         * 
         * @param {string[]} imageZoneIds The array of image zone ids.
         * @return {Commerce.Model.Entities.ImageZone[]} An array of image zones.
         */
        public getImageZones(imageZoneIds: string[]): ImageZone[] {
            var results: ImageZone[] = [];
            if (!ObjectExtensions.isNullOrUndefined(this._tillLayoutResponse) && !ObjectExtensions.isNullOrUndefined(this._tillLayoutResponse.ImageZones) &&
                this._tillLayoutResponse.ImageZones.length > 0) {
                results = this._tillLayoutResponse.ImageZones.filter((item) => {
                    return imageZoneIds.indexOf(item.ZoneId) > -1;
                });
            }
            return results;
        }

        /**
         * Gets a array of image zones.
         * 
         * @param {Commerce.Model.Entities.ButtonGrid[]} buttonGrids The array of button grid ids.
         */
        public setButtonGrids(buttonGrids: ButtonGrid[]): void {
            this._allButtonGrids = buttonGrids;
        }

        /**
         * Initializes button grid dictionary.
         * 
         */
        private initializeButtonGridDictionary() {
            this._buttonGridsByZoneId = {};
            this._buttonGridsById = {};

            if (!ObjectExtensions.isNullOrUndefined(this._tillLayoutResponse) &&
                ArrayExtensions.hasElements(this._tillLayoutResponse.ButtonGridZones) &&
                ArrayExtensions.hasElements(this._allButtonGrids)) {
                for (var i = 0; i < this._allButtonGrids.length; i++) {
                    var buttonGrid = this._allButtonGrids[i];

                    for (var j = 0; j < this._tillLayoutResponse.ButtonGridZones.length; j++) {
                        var buttonGridZone = this._tillLayoutResponse.ButtonGridZones[j];
                        if (buttonGridZone.ButtonGridId == buttonGrid.Id) {
                            this._buttonGridsByZoneId[buttonGridZone.ZoneId] = buttonGrid;
                        }
                    }

                    this._buttonGridsById[buttonGrid.Id] = buttonGrid;
                }
            }
        }

        /**
         * Gets a dictionary of button grids by id.
         * 
         * @param {string} buttonGridId The button grid id.
         * @return {Commerce.Model.Entities.ButtonGrid} A dictionary of button grids.
         */
        public getButtonGridById(buttonGridId: string): ButtonGrid {
            if (this._buttonGridsById == null) {
                this.initializeButtonGridDictionary();
            }

            return this._buttonGridsById[buttonGridId];
        }

        /**
         * Gets a dictionary of button grids by zone ids.
         * 
         * @param {string[]} buttonGridZoneIds The list of button grid zone ids.
         * @return {Commerce.Dictionary<Commerce.Model.Entities.ButtonGrid>} A dictionary of button grids.
         */
        public getButtonGridByZoneIds(buttonGridZoneIds: string[]): Dictionary<Entities.ButtonGrid> {
            var buttonGrids: Dictionary<Entities.ButtonGrid> = new Dictionary<Entities.ButtonGrid>();

            if (this._buttonGridsByZoneId == null) {
                this.initializeButtonGridDictionary();
            }

            for (var i = 0; i < buttonGridZoneIds.length; i++) {
                var buttonGrid = this._buttonGridsByZoneId[buttonGridZoneIds[i]];
                if (!ObjectExtensions.isNullOrUndefined(buttonGrid)) {
                    buttonGrids.setItem(buttonGridZoneIds[i], buttonGrid);
                }
            }

            return buttonGrids;
        }

        /**
         * Gets all the custom controls defined in the given view.
         *
         * @param {string} view The view name.
         * @return {CustomControl[]} The collection of custom controls defined in the given view.
         */
        public getCustomControls(view: string): CustomControl[] {
            var customControls: CustomControl[] = [];
            if (StringExtensions.isNullOrWhitespace(view)) {
                return customControls;
            }

            var layout = this.getLayout(view);
            if (ObjectExtensions.isNullOrUndefined(layout)) {
                return customControls;
            }

            layout.DesignerControls.filter(item => item.Type.indexOf("ICustomControl") != -1)
                .forEach(item => customControls.push(<CustomControl>item));

            return customControls;
        }

        /**
         * Gets a layout.
         * 
         * @param {string} view The layout name.
         * @param {string} layoutItemId The item id.
         * @return {Commerce.Model.Entities.Layout} A Layout.
         */
        public getLayoutItem(view: string, layoutItemId: string): Layout {
            if (!Commerce.StringExtensions.isNullOrWhitespace(layoutItemId)) {
                var layout = this.getLayout(view);

                if (!Commerce.ObjectExtensions.isNullOrUndefined(layout)) {
                    if (layout.DesignerControls) {
                        var filteredItems = layout.DesignerControls.filter((item) => {
                            return item.ID == layoutItemId;
                        });

                        if (filteredItems.length > 0) {
                            var item = filteredItems[0];
                            return item;
                        }
                        else {
                            // try find in tab control
                            var tabControls = layout.DesignerControls.filter((item) => {
                                return item.ID == "TabControl";
                            });

                            if (tabControls.length > 0) {
                                var tabControl = tabControls[0];
                                var tabItems = tabControl.TabPages.filter((item) => {
                                    return item.Content.ID == layoutItemId;
                                });
                                if (tabItems.length > 0) {
                                    var item = tabItems[0].Content;
                                    return item;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        /**
         * Gets a layout.
         * 
         * @param {string} view The layout name.
         * @return {Commerce.Model.Entities.Layout} A Layout.
         */
        public getLayout(view: string): Layout {
            if (!Commerce.StringExtensions.isNullOrWhitespace(view)) {
                var screenLayout = <ScreenLayout>this[view];
                if (!screenLayout) {
                    return null;
                }

                return <Layout>screenLayout[this.orientation];
            }

            return null;
        }
    }
}