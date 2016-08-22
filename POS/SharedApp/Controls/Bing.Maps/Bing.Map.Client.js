(function () {
    var loader = {
        loadScript: function (path, done) {
            var url = path;
            var scriptTag = document.createElement("script");
            scriptTag.type = "text/javascript";
            scriptTag.onload = done;
            scriptTag.src = url;
            document.head.appendChild(scriptTag);
        },

        // Load stylesheet.
        loadStyleSheet: function (path, id) {
            var url = path;
            var fileref = document.createElement("link");
            fileref.setAttribute("rel", "stylesheet");
            fileref.setAttribute("type", "text/css");
            fileref.setAttribute("href", url);

            if (id) {
                fileref.setAttribute("id", id);
            }

            document.head.appendChild(fileref);
        }
    };

    var MapWrapper = function () {

        var ErrorEvent = function (errorMessage) {
            this.message = errorMessage;
        };

        var MapMessages = {
            ERROR: "error",
            INFOBOX_HYPERLINK_CLICKED: "infoboxHyperlinkClicked",
            INITIALIZATION_ERROR: "initialization_error",
            LOADED: "loaded",
            READY: "ready",
            SEARCH_SUCCESS: "searchSuccess",
            UPDATE_LOCATIONS: "updateLocations"
        };

        var DistanceUnit = { Miles: 0, Kilometers: 1 };

        var protocol = document.location.protocol.indexOf("ms-appx") !== -1
            ? "ms-appx:"    // in case ms-appx is part of the protocol, we are inside an win8-app
            : document.location.protocol;

        var domain = protocol + "//" + document.location.host;

        var map = null;
        var mapsAPIKey = null;
        var searchManager = null;
        var mapScriptLoadAttempts = 5; // max load attempt count, delay 500 ms between
        var previousZoomValue = null;
        var previousCenter = null;
        var distanceUnitValue = DistanceUnit.Miles;
        var currentCssTheme = "";
        var accentColorLoaded = false;
        var currentTextDirection = null;
        var currentInfobox = null;
        var currentInfoboxClickId = null;
        var currentInfoboxClickEvent = null;
        var currentClickEvent = null;

        var requiredModules = [
            "Microsoft.Maps.Overlays.Style",
            "Microsoft.Maps.Search",
            "Microsoft.Maps.Themes.BingTheme"
        ];

        var posLightThemeElementId = "";
        var posDarkThemeElementId = "";
        var winUILightThemeElementId = "";
        var winUIDarkThemeElementId = "";

        return {
            processMessage: function (msg) {
                if (!msg || !msg.data || msg.origin !== domain) {
                    return;
                }
                var call = JSON.parse(msg.data);

                if (!call.functionName) {
                    throw "Message does not contain a valid function name.";
                }
                var target = this[call.functionName];

                if (typeof target != "function") {
                    throw "The function name does not resolve to an actual function";
                }

                target.apply(this, call.args);
            },

            notifyParent: function (event, args) {
                if (!args) args = {};

                args["event"] = event;
                window.parent.postMessage(JSON.stringify(args), domain);
            },

            addListener: function () {
                window.addEventListener("message", this.processMessage.bind(this), false);
            },

            loadModules: function () {
                for (var moduleIndex in requiredModules) {
                    if (requiredModules.hasOwnProperty(moduleIndex)) {

                        var module = requiredModules[moduleIndex];
                        Microsoft.Maps.loadModule(module, { callback: this.onModuleLoaded.bind(this, module) });
                    }
                }
            },

            onModuleLoaded: function (module) {
                var moduleIndex = requiredModules.indexOf(module);
                requiredModules.splice(moduleIndex, 1);
                if (requiredModules.length === 0) {
                    this.initMap();
                }
            },

            init: function (credentials) {

                // make sure that all is loaded
                if (typeof Microsoft === "undefined" || !Microsoft.Maps.hasOwnProperty("loadModule")) {
                    if (mapScriptLoadAttempts-- > 0) {
                        setTimeout(this.init.bind(this, credentials), 500);
                    } else {
                        this.notifyParent.call(this, MapMessages.INITIALIZATION_ERROR, new ErrorEvent("Map cannot be initialized"));
                    }

                    return;
                }

                if (!credentials) {
                    this.notifyParent.call(this, MapMessages.ERROR, new ErrorEvent("Bing maps api key is missing"));
                    return;
                }

                mapsAPIKey = credentials;

                this.loadModules.call(this);
            },

            initMap: function () {

                map = new Microsoft.Maps.Map(document.getElementById("mapDiv"), {
                    credentials: mapsAPIKey,
                    enableSearchLogo: false,
                    mapTypeId: Microsoft.Maps.MapTypeId.road,
                    customizeOverlays: true,
                    showDualMapTypeSelector: true,
                    theme: new Microsoft.Maps.Themes.BingTheme(),
                    showBreadcrumb: false,
                });

                map.addComponent("searchManager", new Microsoft.Maps.Search.SearchManager(map));
                searchManager = map.getComponent("searchManager");

                previousZoomValue = map.getZoom();
                previousCenter = map.getCenter();

                Microsoft.Maps.Events.addThrottledHandler(map, "viewchangestart", this.viewChangeStart.bind(this), 1000);
                Microsoft.Maps.Events.addThrottledHandler(map, "viewchangeend", this.viewChangeEnd.bind(this), 1000);

                this.notifyParent.call(this, MapMessages.LOADED);
                mapLoaded = true;
            },

            pinLocation: function (latitude, longitude, text) {

                if (!this.checkMap()) {
                    return;
                }

                var location = new Microsoft.Maps.Location(latitude, longitude);
                var pushpin = new Microsoft.Maps.Pushpin(location, { text: text });

                map.entities.push(pushpin);
            },

            removeAllPushpins: function () {

                if (!this.checkMap()) {
                    return;
                }

                for (var i = map.entities.getLength() - 1; i >= 0; i--) {
                    var pushpin = map.entities.get(i);
                    if (pushpin instanceof Microsoft.Maps.Pushpin) {
                        map.entities.removeAt(i);
                    };
                }
            },

            // Remove infobox(es) from Bing maps.
            removeInfobox: function () {
                var element = document.getElementById(currentInfoboxClickId);
                if (element && currentInfoboxClickEvent) {
                    element.removeEventListener("click", currentInfoboxClickEvent);
                }
                currentClickEvent = null;

                if (currentInfobox) {
                    currentInfobox.setOptions({ visible: false });
                    currentInfobox = null;
                }
            },

            // Show infobox on Bing maps.
            setInfobox: function (latitude, longitude, closeButtonId, title, text) {

                if (this.checkMap()) {
                    this.removeInfobox();

                    // A buffer limit to use to specify the infobox must be away from the edges of the map.
                    var buffer = 9;

                    var infoboxOptions = {
                        width: 350, height: 150, showCloseButton: false, zIndex: 0,
                        offset: new Microsoft.Maps.Point(0, buffer), showPointer: true,
                        title: title, description: text
                    };

                    currentInfobox = new Microsoft.Maps.Infobox(map.getCenter(), infoboxOptions);
                    map.entities.push(currentInfobox);

                    var location = new Microsoft.Maps.Location(latitude, longitude);
                    currentInfobox.setLocation(location);

                    // Move map if infobox is partially hidden.
                    var infoboxOffset = currentInfobox.getOffset();
                    var infoboxAnchor = currentInfobox.getAnchor();
                    var infoboxLocation = map.tryLocationToPixel(location, Microsoft.Maps.PixelReference.control);

                    var dx = infoboxLocation.x + infoboxOffset.x - infoboxAnchor.x;
                    var dy = infoboxLocation.y - buffer - infoboxAnchor.y;

                    if (dy < buffer) { // Infobox overlaps with top of map.
                        // Offset in opposite direction.
                        dy *= -1;
 
                        // Add buffer from the top edge of the map.
                        dy += buffer;
                    } else { // If dy is greater than zero than it does not overlap.
                        dy = 0;
                    }
 
                    // Check to see if overlapping with left side of map.
                    if (dx < buffer) {    

                        // Offset in opposite direction.
                        dx *= -1;
 
                        // Add a buffer from the left edge of the map.
                        dx += buffer;
                    } else { // Check to see if overlapping with right side of map.
                        
                        dx = map.getWidth() - infoboxLocation.x + infoboxAnchor.x - currentInfobox.getWidth();
 
                        // If dx is greater than buffer then it does not overlap.
                        if (dx > buffer) {
                            dx = 0;
                        } else {
                            // Add a buffer from the right edge of the map.
                            dx -= buffer;
                        }
                    }
 
                    // Adjust the map so infobox is in view.
                    if (dx !== 0 || dy !== 0) {
                        map.setView({ centerOffset: new Microsoft.Maps.Point(dx, dy), center: map.getCenter() });
                    }

                    if (closeButtonId) {
                        var infoBox = document.getElementById(closeButtonId);
                        if (infoBox) {
                            currentInfoboxClickId = closeButtonId;

                            var self = this;
                            currentClickEvent = function (mouseEvent) {
                                self.removeInfobox();
                            };
                            currentInfoboxClickEvent = currentClickEvent;
                            infoBox.addEventListener("click", currentClickEvent);
                        }
                    }
                }
            },

            setMapView: function (latitude, longitude, zoom) {

                if (!this.checkMap()) {
                    return;
                }

                var view = {};
                if (latitude != null && longitude != null) {
                    view['center'] = new Microsoft.Maps.Location(latitude, longitude);
                }
                if (zoom) {
                    view['zoom'] = zoom;
                }

                map.setView(view);
            },

            searchByAddress: function (address) {
                if (!this.checkMap()) {
                    this.notifyParent.call(this, MapMessages.INITIALIZATION_ERROR, new ErrorEvent("Map cannot be initialized"));
                    return;
                }

                if (!searchManager) return;

                var request = {
                    where: address,
                    count: 1,
                    callback: onSearchSuccess.bind(this),
                    errorCallback: onSearchFailed.bind(this),
                };

                searchManager.geocode(request);

                function onSearchSuccess(result) {
                    if (result) {
                        map.entities.clear();
                        var topResult = result.results && result.results[0];
                        var resultData = {};
                        if (topResult) {
                            map.setView({ center: topResult.location, zoom: 10 });
                            resultData.searchResult = topResult;
                            resultData.radius = this.getExcircleRadius(map.getBounds());
                        }

                        this.notifyParent.call(this, MapMessages.SEARCH_SUCCESS, resultData);
                    }
                }

                function onSearchFailed(result) {
                    this.notifyParent.call(this, MapMessages.ERROR, result);
                }
            },

            checkMap: function () {
                if (map === null) {
                    return false;
                }
                return true;
            },

            viewChangeStart: function () {
                previousZoomValue = map.getZoom();
                previousCenter = map.getCenter();
            },

            viewChangeEnd: function () {
                var newZoomValue = map.getZoom();
                var newBounds = map.getBounds();

                // We are starting search only when panning or when zooming out
                if ((this.isNotZeroLocation(newBounds.center) && this.isNotZeroLocation(previousCenter) && !Microsoft.Maps.Location.areEqual(newBounds.center, previousCenter))
                    || newZoomValue < previousZoomValue) {
                    this.notifyParent.call(this, MapMessages.UPDATE_LOCATIONS, { longitude: newBounds.center.longitude, latitude: newBounds.center.latitude, radius: this.getExcircleRadius(newBounds) });

                    previousZoomValue = newZoomValue;
                    previousCenter = newBounds.center;
                }
            },

            isNotZeroLocation: function (location) {

                // The smallest latitude and longitude value that we are taking into account as non-zero
                // other values less than EPSILON are considered as equal to zero
                var EPSILON = 0.0001;

                return Math.abs(location.latitude) > EPSILON && Math.abs(location.longitude) > EPSILON;
            },

            // This method is used for maximum distance calculation when searching stores.
            // Returns the radius of the excircle of the current map view based on rib with max length.
            getExcircleRadius: function (locationRect) {

                var conversionValue = this.getUnitConversion();

                var width = Math.abs(conversionValue * locationRect.width * Math.cos(locationRect.center.latitude * (Math.PI / 180)));
                var height = conversionValue * locationRect.height;

                var maxLength = Math.max(width, height);

                return maxLength / Math.sqrt(2);
            },

            getUnitConversion: function () {
                var MILES_PER_1_LATITUDE_DEGREE = 69.055; // average number of miles per latitude degree. Actual value changes from 68.703 to 69.407 miles
                var KILOMETERS_PER_1_LATITUDE_DEGREE = 111.133; // average number of kilometers per latitude degree. Actual value changes from 110.567 to 111.699 kilometers

                return distanceUnitValue === DistanceUnit.Miles ? MILES_PER_1_LATITUDE_DEGREE : KILOMETERS_PER_1_LATITUDE_DEGREE;
            },

            //Initialize CSS element theme identifier.
            initializeThemeElementIds: function (posLightThemeId, posDarkThemeId, winUILightThemeId, winUIDarkThemeId) {
                posLightThemeElementId = posLightThemeId,
                posDarkThemeElementId = posDarkThemeId,
                winUILightThemeElementId = winUILightThemeId,
                winUIDarkThemeElementId = winUIDarkThemeId
            },

            // Remove stylesheet by identifier.
            removeStyleSheetById: function (id) {
                $("head link[id=" + id + "]").remove();
            },

            // Apply CSS theme.
            applyCSSTheme: function (theme) {
                if (currentCssTheme === theme) {
                    return;
                }

                var infoboxSelector = "body";
                if (theme === "dark") {
                    loader.loadStyleSheet("../../Libraries/winjs/css/ui-dark.css", winUIDarkThemeElementId);
                    loader.loadStyleSheet("../../Stylesheets/Themes/PosDarkTheme.css", posDarkThemeElementId);

                    //apply selected theme on infobox
                    $(infoboxSelector).removeClass(currentCssTheme);
                    $(infoboxSelector).addClass(theme);

                    currentCssTheme = theme;

                    // after loading the correct CSS files, remove the unnecessary ones, if applicable
                    if (document.getElementById(posLightThemeElementId)) {
                        this.removeStyleSheetById(posLightThemeElementId);
                    }
                    if (document.getElementById(winUILightThemeElementId)) {
                        this.removeStyleSheetById(winUILightThemeElementId);
                    }
                } else if (theme === "light") {
                    loader.loadStyleSheet("../../Libraries/winjs/css/ui-light.css", winUILightThemeElementId);
                    loader.loadStyleSheet("../../Stylesheets/Themes/PosLightTheme.css", posLightThemeElementId);

                    //apply selected theme on infobox
                    $(infoboxSelector).removeClass(currentCssTheme);
                    $(infoboxSelector).addClass(theme);
                    currentCssTheme = theme;

                    // after loading the correct CSS files, remove the unnecessary ones, if applicable
                    if (document.getElementById(posDarkThemeElementId)) {
                        this.removeStyleSheetById(posDarkThemeElementId);
                    }
                    if (document.getElementById(winUIDarkThemeElementId)) {
                        this.removeStyleSheetById(winUIDarkThemeElementId);
                    }
                }
                loader.loadStyleSheet("../../Stylesheets/Main.min.css");
            },

            // Apply accent color.
            applyAccentColor: function (accentColor) {
                if (accentColorLoaded) {
                    return;
                }

                var styleSheet = null;
                for (var i = 0; i < document.styleSheets.length; i++) {
                    if (document.styleSheets[i].href && document.styleSheets[i].href.match("Main.min.css$")) {
                        styleSheet = document.styleSheets[i];
                        break;
                    }
                }

                if (!styleSheet) {
                    return;
                }

                if (styleSheet.addRule) {
                    styleSheet.addRule("a, a:active, a:hover", "color: " + accentColor + " !important");
                } else if (styleSheet.insertRule) {
                    styleSheet.insertRule("a, a:active, a:hover { color: " + accentColor + " !important }");
                }

                accentColorLoaded = true;
            },

            // Apply text direction.
            applyTextDirection: function (textDirection) {
                if (currentTextDirection === textDirection) {
                    return;
                }

                $("body").attr("dir", textDirection);
                currentTextDirection = textDirection;
            },

            start: function () {
                MapWrapper.addListener();
                this.notifyParent.call(this, MapMessages.READY);
            }
        };
    }();

    // load css style sheets
    loader.loadScript('../../Libraries/jQuery.min.js', function () {
        MapWrapper.start();
    });
})();
// SIG // Begin signature block
// SIG // MIIdrAYJKoZIhvcNAQcCoIIdnTCCHZkCAQExCzAJBgUr
// SIG // DgMCGgUAMGcGCisGAQQBgjcCAQSgWTBXMDIGCisGAQQB
// SIG // gjcCAR4wJAIBAQQQEODJBs441BGiowAQS9NQkAIBAAIB
// SIG // AAIBAAIBAAIBADAhMAkGBSsOAwIaBQAEFMc6muIiTnCI
// SIG // YipRE0I+FcBwPGwSoIIYZDCCBMMwggOroAMCAQICEzMA
// SIG // AACb4HQ3yz1NjS4AAAAAAJswDQYJKoZIhvcNAQEFBQAw
// SIG // dzELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0
// SIG // b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1p
// SIG // Y3Jvc29mdCBDb3Jwb3JhdGlvbjEhMB8GA1UEAxMYTWlj
// SIG // cm9zb2Z0IFRpbWUtU3RhbXAgUENBMB4XDTE2MDMzMDE5
// SIG // MjEyOVoXDTE3MDYzMDE5MjEyOVowgbMxCzAJBgNVBAYT
// SIG // AlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQH
// SIG // EwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29y
// SIG // cG9yYXRpb24xDTALBgNVBAsTBE1PUFIxJzAlBgNVBAsT
// SIG // Hm5DaXBoZXIgRFNFIEVTTjo3MjhELUM0NUYtRjlFQjEl
// SIG // MCMGA1UEAxMcTWljcm9zb2Z0IFRpbWUtU3RhbXAgU2Vy
// SIG // dmljZTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoC
// SIG // ggEBAI2j4s+Bi9fLvwOiYPY7beLUGLA3BdWNNpwOc85N
// SIG // f6IQsnxDeywYV7ysp6aGfXmhtd4yZvmO/CDNq3N3z3ed
// SIG // b2Cca3jzxa2pvVtMK1WqUoBBQ0FmmaXwMGiGug8hch/D
// SIG // dT+SdsEA15ksqFk/wWKRbQn2ztMiui0An2bLU9HKVjpY
// SIG // TCGyhaOYZYzHiUpFWHurU0CfjGqyBcX+HuL/CqGootvL
// SIG // IY18lTDeMReKDelfzEJwyqQVFG6ED8LC/WwCTJOxTLbO
// SIG // tuzitc2aGhD1SOVXEHfqgd1fhEIycETJyryw+/dIOdhg
// SIG // dUmts79odC6UDhy+wXBydBAOzNtrUB8x6jT6bD0CAwEA
// SIG // AaOCAQkwggEFMB0GA1UdDgQWBBSWlbGeE1O6WCFGNOJ8
// SIG // xzlKbCDwdzAfBgNVHSMEGDAWgBQjNPjZUkZwCu1A+3b7
// SIG // syuwwzWzDzBUBgNVHR8ETTBLMEmgR6BFhkNodHRwOi8v
// SIG // Y3JsLm1pY3Jvc29mdC5jb20vcGtpL2NybC9wcm9kdWN0
// SIG // cy9NaWNyb3NvZnRUaW1lU3RhbXBQQ0EuY3JsMFgGCCsG
// SIG // AQUFBwEBBEwwSjBIBggrBgEFBQcwAoY8aHR0cDovL3d3
// SIG // dy5taWNyb3NvZnQuY29tL3BraS9jZXJ0cy9NaWNyb3Nv
// SIG // ZnRUaW1lU3RhbXBQQ0EuY3J0MBMGA1UdJQQMMAoGCCsG
// SIG // AQUFBwMIMA0GCSqGSIb3DQEBBQUAA4IBAQAhHbNT6TtG
// SIG // gaH6KhPjWiAkunalO7Z3yJFyBNbq/tKbIi+TCKKwbu8C
// SIG // pblWXv1l9o0Sfeon3j+guC4zMteWWj/DdDnJD6m2utr+
// SIG // EGjPiP2PIN6ysdZdKJMnt8IHpEclZbtS1XFNKWnoC1DH
// SIG // jJWWoF6sNzkC1V7zVCh5cdsXw0P8zWor+Q85QER8LGjI
// SIG // 0oHomSKrIFbm5O8khptmVk474u64ZPfln8p1Cu58lp9Z
// SIG // 4aygt9ZpvUIm0vWlh1IB7Cl++wW05tiXfBOAcTVfkybn
// SIG // 5F90lXF8A421H3X1orZhPe7EbIleZAR/KUts1EjqSkpM
// SIG // 54JutTq/VyYRyHiA1YDNDrtkMIIGBzCCA++gAwIBAgIK
// SIG // YRZoNAAAAAAAHDANBgkqhkiG9w0BAQUFADBfMRMwEQYK
// SIG // CZImiZPyLGQBGRYDY29tMRkwFwYKCZImiZPyLGQBGRYJ
// SIG // bWljcm9zb2Z0MS0wKwYDVQQDEyRNaWNyb3NvZnQgUm9v
// SIG // dCBDZXJ0aWZpY2F0ZSBBdXRob3JpdHkwHhcNMDcwNDAz
// SIG // MTI1MzA5WhcNMjEwNDAzMTMwMzA5WjB3MQswCQYDVQQG
// SIG // EwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UE
// SIG // BxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENv
// SIG // cnBvcmF0aW9uMSEwHwYDVQQDExhNaWNyb3NvZnQgVGlt
// SIG // ZS1TdGFtcCBQQ0EwggEiMA0GCSqGSIb3DQEBAQUAA4IB
// SIG // DwAwggEKAoIBAQCfoWyx39tIkip8ay4Z4b3i48WZUSNQ
// SIG // rc7dGE4kD+7Rp9FMrXQwIBHrB9VUlRVJlBtCkq6YXDAm
// SIG // 2gBr6Hu97IkHD/cOBJjwicwfyzMkh53y9GccLPx754gd
// SIG // 6udOo6HBI1PKjfpFzwnQXq/QsEIEovmmbJNn1yjcRlOw
// SIG // htDlKEYuJ6yGT1VSDOQDLPtqkJAwbofzWTCd+n7Wl7Po
// SIG // IZd++NIT8wi3U21StEWQn0gASkdmEScpZqiX5NMGgUqi
// SIG // +YSnEUcUCYKfhO1VeP4Bmh1QCIUAEDBG7bfeI0a7xC1U
// SIG // n68eeEExd8yb3zuDk6FhArUdDbH895uyAc4iS1T/+QXD
// SIG // wiALAgMBAAGjggGrMIIBpzAPBgNVHRMBAf8EBTADAQH/
// SIG // MB0GA1UdDgQWBBQjNPjZUkZwCu1A+3b7syuwwzWzDzAL
// SIG // BgNVHQ8EBAMCAYYwEAYJKwYBBAGCNxUBBAMCAQAwgZgG
// SIG // A1UdIwSBkDCBjYAUDqyCYEBWJ5flJRP8KuEKU5VZ5KSh
// SIG // Y6RhMF8xEzARBgoJkiaJk/IsZAEZFgNjb20xGTAXBgoJ
// SIG // kiaJk/IsZAEZFgltaWNyb3NvZnQxLTArBgNVBAMTJE1p
// SIG // Y3Jvc29mdCBSb290IENlcnRpZmljYXRlIEF1dGhvcml0
// SIG // eYIQea0WoUqgpa1Mc1j0BxMuZTBQBgNVHR8ESTBHMEWg
// SIG // Q6BBhj9odHRwOi8vY3JsLm1pY3Jvc29mdC5jb20vcGtp
// SIG // L2NybC9wcm9kdWN0cy9taWNyb3NvZnRyb290Y2VydC5j
// SIG // cmwwVAYIKwYBBQUHAQEESDBGMEQGCCsGAQUFBzAChjho
// SIG // dHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2NlcnRz
// SIG // L01pY3Jvc29mdFJvb3RDZXJ0LmNydDATBgNVHSUEDDAK
// SIG // BggrBgEFBQcDCDANBgkqhkiG9w0BAQUFAAOCAgEAEJeK
// SIG // w1wDRDbd6bStd9vOeVFNAbEudHFbbQwTq86+e4+4LtQS
// SIG // ooxtYrhXAstOIBNQmd16QOJXu69YmhzhHQGGrLt48ovQ
// SIG // 7DsB7uK+jwoFyI1I4vBTFd1Pq5Lk541q1YDB5pTyBi+F
// SIG // A+mRKiQicPv2/OR4mS4N9wficLwYTp2OawpylbihOZxn
// SIG // LcVRDupiXD8WmIsgP+IHGjL5zDFKdjE9K3ILyOpwPf+F
// SIG // ChPfwgphjvDXuBfrTot/xTUrXqO/67x9C0J71FNyIe4w
// SIG // yrt4ZVxbARcKFA7S2hSY9Ty5ZlizLS/n+YWGzFFW6J1w
// SIG // lGysOUzU9nm/qhh6YinvopspNAZ3GmLJPR5tH4LwC8cs
// SIG // u89Ds+X57H2146SodDW4TsVxIxImdgs8UoxxWkZDFLyz
// SIG // s7BNZ8ifQv+AeSGAnhUwZuhCEl4ayJ4iIdBD6Svpu/RI
// SIG // zCzU2DKATCYqSCRfWupW76bemZ3KOm+9gSd0BhHudiG/
// SIG // m4LBJ1S2sWo9iaF2YbRuoROmv6pH8BJv/YoybLL+31HI
// SIG // jCPJZr2dHYcSZAI9La9Zj7jkIeW1sMpjtHhUBdRBLlCs
// SIG // lLCleKuzoJZ1GtmShxN1Ii8yqAhuoFuMJb+g74TKIdbr
// SIG // Hk/Jmu5J4PcBZW+JC33Iacjmbuqnl84xKf8OxVtc2E0b
// SIG // odj6L54/LlUWa8kTo/0wggYQMIID+KADAgECAhMzAAAA
// SIG // ZEeElIbbQRk4AAAAAABkMA0GCSqGSIb3DQEBCwUAMH4x
// SIG // CzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9u
// SIG // MRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNy
// SIG // b3NvZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jv
// SIG // c29mdCBDb2RlIFNpZ25pbmcgUENBIDIwMTEwHhcNMTUx
// SIG // MDI4MjAzMTQ2WhcNMTcwMTI4MjAzMTQ2WjCBgzELMAkG
// SIG // A1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAO
// SIG // BgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29m
// SIG // dCBDb3Jwb3JhdGlvbjENMAsGA1UECxMETU9QUjEeMBwG
// SIG // A1UEAxMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMIIBIjAN
// SIG // BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAky7a2OY+
// SIG // mNkbD2RfTahYTRQ793qE/DwRMTrvicJKLUGlSF3dEp7v
// SIG // q2YoNNV9KlV7TE2K8sDxstNSFYu2swi4i1AL3X/7agmg
// SIG // 3GcExPHfvHUYIEC+eCyZVt3u9S7dPkL5Wh8wrgEUirCC
// SIG // tVGg4m1l/vcYCo0wbU06p8XzNi3uXyygkgCxHEziy/f/
// SIG // JCV/14/A3ZduzrIXtsccRKckyn6B5uYxuRbZXT7RaO6+
// SIG // zUjQhiyu3A4hwcCKw+4bk1kT9sY7gHIYiFP7q78wPqB3
// SIG // vVKIv3rY6LCTraEbjNR+phBQEL7hyBxk+ocu+8RHZhbA
// SIG // hHs2r1+6hURsAg8t4LAOG6I+JQIDAQABo4IBfzCCAXsw
// SIG // HwYDVR0lBBgwFgYIKwYBBQUHAwMGCisGAQQBgjdMCAEw
// SIG // HQYDVR0OBBYEFFhWcQTwvbsz9YNozOeARvdXr9IiMFEG
// SIG // A1UdEQRKMEikRjBEMQ0wCwYDVQQLEwRNT1BSMTMwMQYD
// SIG // VQQFEyozMTY0Mis0OWU4YzNmMy0yMzU5LTQ3ZjYtYTNi
// SIG // ZS02YzhjNDc1MWM0YjYwHwYDVR0jBBgwFoAUSG5k5VAF
// SIG // 04KqFzc3IrVtqMp1ApUwVAYDVR0fBE0wSzBJoEegRYZD
// SIG // aHR0cDovL3d3dy5taWNyb3NvZnQuY29tL3BraW9wcy9j
// SIG // cmwvTWljQ29kU2lnUENBMjAxMV8yMDExLTA3LTA4LmNy
// SIG // bDBhBggrBgEFBQcBAQRVMFMwUQYIKwYBBQUHMAKGRWh0
// SIG // dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2lvcHMvY2Vy
// SIG // dHMvTWljQ29kU2lnUENBMjAxMV8yMDExLTA3LTA4LmNy
// SIG // dDAMBgNVHRMBAf8EAjAAMA0GCSqGSIb3DQEBCwUAA4IC
// SIG // AQCI4gxkQx3dXK6MO4UktZ1A1r1mrFtXNdn06DrARZkQ
// SIG // Tdu0kOTLdlGBCfCzk0309RLkvUgnFKpvLddrg9TGp3n8
// SIG // 0yUbRsp2AogyrlBU+gP5ggHFi7NjGEpj5bH+FDsMw9Py
// SIG // gLg8JelgsvBVudw1SgUt625nY7w1vrwk+cDd58TvAyJQ
// SIG // FAW1zJ+0ySgB9lu2vwg0NKetOyL7dxe3KoRLaztUcqXo
// SIG // YW5CkI+Mv3m8HOeqlhyfFTYxPB5YXyQJPKQJYh8zC9b9
// SIG // 0JXLT7raM7mQ94ygDuFmlaiZ+QSUR3XVupdEngrmZgUB
// SIG // 5jX13M+Pl2Vv7PPFU3xlo3Uhj1wtupNC81epoxGhJ0tR
// SIG // uLdEajD/dCZ0xIniesRXCKSC4HCL3BMnSwVXtIoj/QFy
// SIG // mFYwD5+sAZuvRSgkKyD1rDA7MPcEI2i/Bh5OMAo9App4
// SIG // sR0Gp049oSkXNhvRi/au7QG6NJBTSBbNBGJG8Qp+5QTh
// SIG // KoQUk8mj0ugr4yWRsA9JTbmqVw7u9suB5OKYBMUN4hL/
// SIG // yI+aFVsE/KJInvnxSzXJ1YHka45ADYMKAMl+fLdIqm3n
// SIG // x6rIN0RkoDAbvTAAXGehUCsIod049A1T3IJyUJXt3OsT
// SIG // d3WabhIBXICYfxMg10naaWcyUePgW3+VwP0XLKu4O1+8
// SIG // ZeGyaDSi33GnzmmyYacX3BTqMDCCB3owggVioAMCAQIC
// SIG // CmEOkNIAAAAAAAMwDQYJKoZIhvcNAQELBQAwgYgxCzAJ
// SIG // BgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAw
// SIG // DgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
// SIG // ZnQgQ29ycG9yYXRpb24xMjAwBgNVBAMTKU1pY3Jvc29m
// SIG // dCBSb290IENlcnRpZmljYXRlIEF1dGhvcml0eSAyMDEx
// SIG // MB4XDTExMDcwODIwNTkwOVoXDTI2MDcwODIxMDkwOVow
// SIG // fjELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0
// SIG // b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1p
// SIG // Y3Jvc29mdCBDb3Jwb3JhdGlvbjEoMCYGA1UEAxMfTWlj
// SIG // cm9zb2Z0IENvZGUgU2lnbmluZyBQQ0EgMjAxMTCCAiIw
// SIG // DQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAKvw+nIQ
// SIG // HC6t2G6qghBNNLrytlghn0IbKmvpWlCquAY4GgRJun/D
// SIG // DB7dN2vGEtgL8DjCmQawyDnVARQxQtOJDXlkh36UYCRs
// SIG // r55JnOloXtLfm1OyCizDr9mpK656Ca/XllnKYBoF6WZ2
// SIG // 6DJSJhIv56sIUM+zRLdd2MQuA3WraPPLbfM6XKEW9Ea6
// SIG // 4DhkrG5kNXimoGMPLdNAk/jj3gcN1Vx5pUkp5w2+oBN3
// SIG // vpQ97/vjK1oQH01WKKJ6cuASOrdJXtjt7UORg9l7snuG
// SIG // G9k+sYxd6IlPhBryoS9Z5JA7La4zWMW3Pv4y07MDPbGy
// SIG // r5I4ftKdgCz1TlaRITUlwzluZH9TupwPrRkjhMv0ugOG
// SIG // jfdf8NBSv4yUh7zAIXQlXxgotswnKDglmDlKNs98sZKu
// SIG // HCOnqWbsYR9q4ShJnV+I4iVd0yFLPlLEtVc/JAPw0Xpb
// SIG // L9Uj43BdD1FGd7P4AOG8rAKCX9vAFbO9G9RVS+c5oQ/p
// SIG // I0m8GLhEfEXkwcNyeuBy5yTfv0aZxe/CHFfbg43sTUkw
// SIG // p6uO3+xbn6/83bBm4sGXgXvt1u1L50kppxMopqd9Z4Dm
// SIG // imJ4X7IvhNdXnFy/dygo8e1twyiPLI9AN0/B4YVEicQJ
// SIG // TMXUpUMvdJX3bvh4IFgsE11glZo+TzOE2rCIF96eTvSW
// SIG // sLxGoGyY0uDWiIwLAgMBAAGjggHtMIIB6TAQBgkrBgEE
// SIG // AYI3FQEEAwIBADAdBgNVHQ4EFgQUSG5k5VAF04KqFzc3
// SIG // IrVtqMp1ApUwGQYJKwYBBAGCNxQCBAweCgBTAHUAYgBD
// SIG // AEEwCwYDVR0PBAQDAgGGMA8GA1UdEwEB/wQFMAMBAf8w
// SIG // HwYDVR0jBBgwFoAUci06AjGQQ7kUBU7h6qfHMdEjiTQw
// SIG // WgYDVR0fBFMwUTBPoE2gS4ZJaHR0cDovL2NybC5taWNy
// SIG // b3NvZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvTWljUm9v
// SIG // Q2VyQXV0MjAxMV8yMDExXzAzXzIyLmNybDBeBggrBgEF
// SIG // BQcBAQRSMFAwTgYIKwYBBQUHMAKGQmh0dHA6Ly93d3cu
// SIG // bWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljUm9vQ2Vy
// SIG // QXV0MjAxMV8yMDExXzAzXzIyLmNydDCBnwYDVR0gBIGX
// SIG // MIGUMIGRBgkrBgEEAYI3LgMwgYMwPwYIKwYBBQUHAgEW
// SIG // M2h0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2lvcHMv
// SIG // ZG9jcy9wcmltYXJ5Y3BzLmh0bTBABggrBgEFBQcCAjA0
// SIG // HjIgHQBMAGUAZwBhAGwAXwBwAG8AbABpAGMAeQBfAHMA
// SIG // dABhAHQAZQBtAGUAbgB0AC4gHTANBgkqhkiG9w0BAQsF
// SIG // AAOCAgEAZ/KGpZjgVHkaLtPYdGcimwuWEeFjkplCln3S
// SIG // eQyQwWVfLiw++MNy0W2D/r4/6ArKO79HqaPzadtjvyI1
// SIG // pZddZYSQfYtGUFXYDJJ80hpLHPM8QotS0LD9a+M+By4p
// SIG // m+Y9G6XUtR13lDni6WTJRD14eiPzE32mkHSDjfTLJgJG
// SIG // KsKKELukqQUMm+1o+mgulaAqPyprWEljHwlpblqYluSD
// SIG // 9MCP80Yr3vw70L01724lruWvJ+3Q3fMOr5kol5hNDj0L
// SIG // 8giJ1h/DMhji8MUtzluetEk5CsYKwsatruWy2dsViFFF
// SIG // WDgycScaf7H0J/jeLDogaZiyWYlobm+nt3TDQAUGpgEq
// SIG // KD6CPxNNZgvAs0314Y9/HG8VfUWnduVAKmWjw11SYobD
// SIG // HWM2l4bf2vP48hahmifhzaWX0O5dY0HjWwechz4GdwbR
// SIG // BrF1HxS+YWG18NzGGwS+30HHDiju3mUv7Jf2oVyW2ADW
// SIG // oUa9WfOXpQlLSBCZgB/QACnFsZulP0V3HjXG0qKin3p6
// SIG // IvpIlR+r+0cjgPWe+L9rt0uX4ut1eBrs6jeZeRhL/9az
// SIG // I2h15q/6/IvrC4DqaTuv/DDtBEyO3991bWORPdGdVk5P
// SIG // v4BXIqF4ETIheu9BCrE/+6jMpF3BoYibV3FWTkhFwELJ
// SIG // m3ZbCoBIa/15n8G9bW1qyVJzEw16UM0xggS0MIIEsAIB
// SIG // ATCBlTB+MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2Fz
// SIG // aGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UE
// SIG // ChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSgwJgYDVQQD
// SIG // Ex9NaWNyb3NvZnQgQ29kZSBTaWduaW5nIFBDQSAyMDEx
// SIG // AhMzAAAAZEeElIbbQRk4AAAAAABkMAkGBSsOAwIaBQCg
// SIG // gcgwGQYJKoZIhvcNAQkDMQwGCisGAQQBgjcCAQQwHAYK
// SIG // KwYBBAGCNwIBCzEOMAwGCisGAQQBgjcCARUwIwYJKoZI
// SIG // hvcNAQkEMRYEFOcKJGfHwZMeoa+BFPtMmG5WmfrPMGgG
// SIG // CisGAQQBgjcCAQwxWjBYoCaAJABCAGkAbgBnAC4ATQBh
// SIG // AHAALgBDAGwAaQBlAG4AdAAuAGoAc6EugCxodHRwOi8v
// SIG // d3d3Lk1pY3Jvc29mdC5jb20vTWljcm9zb2Z0RHluYW1p
// SIG // Y3MvIDANBgkqhkiG9w0BAQEFAASCAQBEmvrRyjE7WwBK
// SIG // pdNJEquOLh3hWbrfuiuvLZMYwrFdX0N/bj1XnFuPsH3o
// SIG // GyAx0Z/TBUD5DnaFy7YV/pfhmdD8BWV4yb+PR/DFIwf2
// SIG // Xc2vqccGc8iOjEIkCyCKM2FkgSdx2OWEAw5Ulr7Y5lWJ
// SIG // P5dyJdacXFyYecaz3qCLmy7SjeANY0DQgYSsGs+vkPSI
// SIG // PJcToJLbgF+iL4XaVq2RTdZjvCO8Rv7/cANZsNtB98uX
// SIG // 24HoNJMvC4KQRiq+ZcUkO9VvbFsRxF5Sb1EYmojPqAAy
// SIG // XO1CG9jz/lKQ4z1q6EtuoX2OW1x+Hku1dxWP5Ahfh/Yv
// SIG // hMKsb6DzOZCDrjm0Amg+oYICKDCCAiQGCSqGSIb3DQEJ
// SIG // BjGCAhUwggIRAgEBMIGOMHcxCzAJBgNVBAYTAlVTMRMw
// SIG // EQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRt
// SIG // b25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRp
// SIG // b24xITAfBgNVBAMTGE1pY3Jvc29mdCBUaW1lLVN0YW1w
// SIG // IFBDQQITMwAAAJvgdDfLPU2NLgAAAAAAmzAJBgUrDgMC
// SIG // GgUAoF0wGAYJKoZIhvcNAQkDMQsGCSqGSIb3DQEHATAc
// SIG // BgkqhkiG9w0BCQUxDxcNMTYwNzIxMjEwNjUxWjAjBgkq
// SIG // hkiG9w0BCQQxFgQUzeIz8L8f3mPYMG7sFS1VV/TYpW4w
// SIG // DQYJKoZIhvcNAQEFBQAEggEARY0nLBly5hbPmjngP/ab
// SIG // oVhzoEhWwH6OSasME1br4XC8keM2ZwkRamJs8Vey5ttC
// SIG // xnXnHVWP3UCQSDFFMFv0G8pGAGxGTLLNgXjd3tW8myf1
// SIG // CdVc5B6SVE6DKjd3Sey5ww9Y03NtzgZXs6pgqFF46A3c
// SIG // pefEjf+WawjsiDviFk9V/+fb95DfAw8MoDLFphTszpl3
// SIG // ZAWImwBRiNsJCSQs6Iyprk4YCExG4Xp0ZKM1JDaFl9Q+
// SIG // I92uWJGKjoqu4buG+ZVX1QHY7lU2LMj3JhwYuDjiKOOH
// SIG // t4qfu/bC5No8nH+s3OrmOEYfAvsQ8v3XEJKESAv7An3P
// SIG // 4WUBpMv0calXPw==
// SIG // End signature block
