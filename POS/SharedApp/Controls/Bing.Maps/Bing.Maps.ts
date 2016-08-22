/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.Core.d.ts'/>

module Commerce.Controls.Bing {

    // List of methods implemented in iframe
    export class MapMethods {

        public static DEFAULT_ZOOM_LEVEL: number = 13;
        public static SHOW_ALL_ZOOM_LEVEL: number = 8;
        public static PIN_LOCATION = "pinLocation";
        public static SET_MAP_VIEW = "setMapView";
        public static INIT = "init";
        public static SEARCH_BY_ADDRESS = "searchByAddress";
        public static REMOVE_PUSHPINS = "removeAllPushpins";
        public static ADD_INFOBOX = "setInfobox";
        public static REMOVE_INFOBOX = "removeInfobox";
        public static APPLY_THEME = "applyCSSTheme";
        public static APPLY_ACCENT_COLOR = "applyAccentColor";
        public static APPLY_TEXT_DIRECTION = "applyTextDirection";
        public static INITIALIZE_THEME_ELEMENT_IDS = "initializeThemeElementIds";
    }

    export class MapEvents {
        public static ERROR: string = "error";
        public static INFOBOX_HYPERLINK_CLICKED: string = "infoboxHyperlinkClicked";
        public static INITIALIZATION_ERROR: string = "initialization_error";
        public static LOADED: string = "loaded";
        public static READY: string = "ready";
        public static SEARCH_SUCCESS: string = "searchSuccess";
        public static UPDATE_LOCATIONS: string = "updateLocations";
    }

    export class MapController implements IDisposable {
        private static LISTENER_TYPE: string = "message";
        private static _currentMessageEventListener: (event: any) => any = null; //only one event message listener on bing maps at one time.
        private _content: any;
        private _map: HTMLIFrameElement;
        private _eventHandlers: Dictionary<Function>;
        private _cssSettingsInitialized: boolean = false;

        private static _accentColorSubscription: IDisposable = null;
        private static _themeSubscription: IDisposable = null;
        private static _textDirectionSubscription: IDisposable = null;

        private accentColor: Observable<string> = ko.observable(StringExtensions.EMPTY);
        private theme: Observable<string> = ko.observable(StringExtensions.EMPTY);
        private textDirection: Observable<string> = ko.observable(StringExtensions.EMPTY);

        constructor(content: any, map: HTMLIFrameElement, eventHandlers: Dictionary<Function>) {
            this._map = map;
            this._eventHandlers = eventHandlers;
            this._content = content;

            if (MapController._accentColorSubscription) {
                MapController._accentColorSubscription.dispose();
            }
            MapController._accentColorSubscription = CSSHelpers.accentColor.subscribe((newAccentColor: string) => {
                this.postMessage(MapMethods.APPLY_ACCENT_COLOR, [newAccentColor]);
            }, this);

            if (MapController._themeSubscription) {
                MapController._themeSubscription.dispose();
            }
            MapController._themeSubscription = CSSHelpers.currentThemeLoaded.subscribe((newTheme: string) => {
                this.postMessage(MapMethods.APPLY_THEME, [newTheme]);
            }, this);

            if (MapController._textDirectionSubscription) {
                MapController._textDirectionSubscription.dispose();
            }
            MapController._textDirectionSubscription = CSSHelpers.currentTextDir.subscribe((newTextDirection: string) => {
                this.postMessage(MapMethods.APPLY_TEXT_DIRECTION, [newTextDirection]);
            }, this);
        }

        private postMessage(functionName: string, args?: any[]): void {
            if (ObjectExtensions.isNullOrUndefined(this._map) || ObjectExtensions.isNullOrUndefined(this._map.contentWindow)) return;

            this._map.contentWindow.postMessage(JSON.stringify({
                functionName: functionName, 
                args: args
            }), "*");
        }

        /**
         * Set CSS Settings after map is loaded.
         */
        private initializeCSSSettings(): void {
            this.postMessage(MapMethods.APPLY_ACCENT_COLOR, [CSSHelpers.accentColor()]);
            this.postMessage(MapMethods.APPLY_TEXT_DIRECTION, [CSSHelpers.currentTextDir()]);
            this.postMessage(MapMethods.APPLY_THEME, [CSSHelpers.currentThemeLoaded()]);

            this._cssSettingsInitialized = true;
        }

        /**
         * Add event listener type message to bing maps controller.
         *
         * @param {(event: any) => any} listener The listener to be added on bing maps controller.
         */
        public addMessageEventListener(listener: (event: any) => any): void {
            this.removeCurrentEventListener();
            MapController._currentMessageEventListener = listener;
            window.addEventListener(MapController.LISTENER_TYPE, listener, false);
        }

        /* 
        * This function catches messages from iframe and redirect them to 
        * specified handler on page which using map control
        */
        public processMessage(msg: MessageEvent): void {
            if (ObjectExtensions.isNullOrUndefined(msg) || ObjectExtensions.isNullOrUndefined(msg.data)) {
                return;
            }

            if (ObjectExtensions.isNullOrUndefined(this._map)) {
                return;
            }

            // we must check that is the case and the message comes from the iframe
            // each browser will return a different origin depending on their implementation of the sandbox iframe
            if (msg.source !== this._map.contentWindow) {
                return;
            }

            var call: any = JSON.parse(msg.data);

            if (this._eventHandlers.hasItem(call.event)) {
                this._eventHandlers.getItem(call.event).call(this._content, call);
            }

            switch (call.event) {
                case MapEvents.LOADED:
                    if (!this._cssSettingsInitialized) {
                        this.postMessage(MapMethods.INITIALIZE_THEME_ELEMENT_IDS, [CSSHelpers.posLightThemeElementId,
                            CSSHelpers.posDarkThemeElementId, CSSHelpers.winUILightThemeElementId, CSSHelpers.winUIDarkThemeElementId]);
                        this.initializeCSSSettings();
                    };
                    break;

                case MapEvents.READY:
                    // map is ready to receive commands - tell it to load
                    this._loadMap();
                    break;
            }
        }

        // Sends message to map control to begin loading
        private _loadMap(): void {
            
            //Throw error if BING maps API key is not provided
            if (ObjectExtensions.isNullOrUndefined(ApplicationContext.Instance.channelConfiguration.BingMapsApiKey)) {
                NotificationHandler.displayErrorMessage('string_29013');
                return;
            }

            this.postMessage(MapMethods.INIT, [ApplicationContext.Instance.channelConfiguration.BingMapsApiKey]);
        }

        // Adds pushpin to the map at specified coords with specified text
        public addMapPin(latitude: number, longitude: number, pinText: string): void {
            this.postMessage(MapMethods.PIN_LOCATION, [latitude, longitude, pinText]);
        }

        // Centers map view to specified coords with specified zoom
        public setMapView(location?: Commerce.Model.Entities.OrgUnitLocation, zoom?: number): void {
            this.postMessage(MapMethods.SET_MAP_VIEW, [location != null ? location.Latitude : null, location != null ? location.Longitude : null, zoom]);
        }

        // Search specified address on map using Bing map search
        public searchByAddress(address: string): void {
            this.postMessage(MapMethods.SEARCH_BY_ADDRESS, [address]);
        }

        // Removes all pushpins from the map
        public removePushpins(): void {
            this.postMessage(MapMethods.REMOVE_PUSHPINS, []);
        }

        /**
         * Show infobox to Bing maps.
         *
         * @param {number} latitude The latitude coordinate.
         * @param {number} longitude The longitude coordinate.
         * @param {string} title The infobox title.
         * @param {string} text The infobox text.
         * @param {string} hyperlinkId The hyperlink identifier to be clicked.
         */
        public addInfoBox(latitude: number, longitude: number, title: string, text: string, hyperlinkId: string): void {
            this.postMessage(MapMethods.ADD_INFOBOX, [latitude, longitude, hyperlinkId, title, text]);
        }

        /**
         * Remove infobox(es) on Bing maps.
         */
        public removeInfobox(): void {
            this.postMessage(MapMethods.REMOVE_INFOBOX, []);
        }

        /**
         * Show store information to infobox.
         *
         * @param {Model.Entities.StoreLocationWrapper} storeWrapper The store entity to be shown.
         * @param {boolean} showDetailsUrl True if hyperlink to store details page is shown, false otherwise.
         */
        public addStoreInfobox(storeWrapper: Model.Entities.StoreLocationWrapper, showDetailsUrl: boolean): void {
            var textContent: string = StringExtensions.EMPTY;
            var closeButtonId: string = "infobox-close";

            textContent += "<h4 id='" + closeButtonId + "' class='closeButton primaryFontColor'>&#xE10A</h4>";
            textContent += "<div class='row'>";
            textContent += "<div>";
            textContent += "<h3 class='ellipsis primaryFontColor'>" + storeWrapper.store.OrgUnitName + "</h3>";
            textContent += "<h4 class='primaryFontColor'>" + storeWrapper.store.OrgUnitNumber + "</h4>";
            textContent += "</div>";
            textContent += "</div>";
            textContent += "<h4 class='ellipsis secondaryFontColor'>" + AddressHelper.getFormattedAddress(storeWrapper.orgUnit.OrgUnitAddress, true) + "</h4>";

            this.addInfoBox(storeWrapper.store.Latitude, storeWrapper.store.Longitude, StringExtensions.EMPTY, textContent, closeButtonId);
        }

        public dispose(): void {
            this._map = null;
            this._content = null;
            this._eventHandlers.clear();
            this.removeCurrentEventListener();
        }

        /**
         * Creates an iframe element where the Bing Maps resides.
         * Iframe element does not work well with JQuery and Internet Explorer, as
         * JQuery would be loaded first before the iframe element that makes
         * the primitive type like Object, String or Array gives you undefined value.
         * Also, the methods on map.html would be executed twice which is not the correct behavior.
         * To encounter the situation, we need to create element on the fly after
         * ancestor controller that will have Bing Maps is loaded. This will make
         * methods on map.html only being executed once, and JQuery will be loaded correctly.
         *
         * @param {string} parentElementId The element identifier of parent element.
         * @param {string} elementId The element identifier of the iframe.
         * @param {string} cssClass [Optional] The css class of the iframe.
         * @param {string} dataBind [Optional] The data binding of the iframe.
         * @returns {HTMLElement} The html element that contains the Bing Maps.
         */
        public static createBingMapsElement(parentElementId: string, elementId: string, cssClass?: string, dataBind?: string): HTMLElement {
            
            var iFrame: JQuery = $("<iframe>").attr("id", elementId);
            iFrame.attr("src", UrlHelper.getWebCompartmentUrl(document, "Controls/Bing.Maps/map.html"));
            iFrame.attr("sandbox", "allow-scripts allow-same-origins");

            if (!StringExtensions.isNullOrWhitespace(cssClass)) {
                iFrame.attr("class", cssClass);
            }

            if (!StringExtensions.isNullOrWhitespace(dataBind)) {
                iFrame.attr("data-bind", dataBind);
            }

            iFrame.appendTo("#" + parentElementId);

            return document.getElementById(elementId);
        }

        private removeCurrentEventListener(): void {
            if (!ObjectExtensions.isNullOrUndefined(MapController._currentMessageEventListener)) {
                window.removeEventListener(MapController.LISTENER_TYPE, MapController._currentMessageEventListener, false);
                MapController._currentMessageEventListener = null;
            }
        }
    }
}