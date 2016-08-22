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

    export class UrlHelper {

        private static PATH_SEPARATOR: string = "/";
        private static PROTOCOL_SEPARATOR: string = "//";
        private static PROTOCOL_SEPARATOR_WITH_COLON: string = "://";
        private static WWAHOST_APP_PROTOCOL_NOCOLON: string = "ms-appx";
        private static WWAHOST_WEBAPP_PROTOCOL: string = "ms-appx-web:";
        private static RETAIL_SERVER: string = "retailserver";
        private static LOCAL_ADDRESS_PATTERN: RegExp = /:\/\/(localhost|127\.0\.0\.1)(:\d+)?/i;
        private static HTTPS_PROTOCOL: string = "HTTPS:";
        private static ARGUMENT_SEPARATOR: string = "&";

        /**
         * Gets the url that allows web files to be loaded into the application.
         * @param {Document} document the HTML document.
         * @param {string} path path for the url.
         * @returns {string} the absolute url (with protocol) representing the desired path.
         */
        public static getWebCompartmentUrl(document: Document, path: string): string {
            if (path.indexOf(UrlHelper.PROTOCOL_SEPARATOR_WITH_COLON) !== -1) {
                throw new Error("Protocol not expected on path.");
            }

            if (path.indexOf(UrlHelper.PROTOCOL_SEPARATOR) === 0) {
                throw new Error("Host not expected on path.");
            }

            var relativePath: boolean = path.charAt(0) !== UrlHelper.PATH_SEPARATOR;

            if (relativePath) {

                var locationPath: string = document.location.pathname;
                var lastPathSeparatorIndex: number = locationPath.lastIndexOf(UrlHelper.PATH_SEPARATOR);

                if (lastPathSeparatorIndex >= 0) {
                    locationPath = locationPath.substring(0, lastPathSeparatorIndex);
                }

                path = locationPath + UrlHelper.PATH_SEPARATOR + path;
            }

            return UrlHelper.getWebAppProtocol(document) + UrlHelper.PROTOCOL_SEPARATOR + document.location.host + path;
        }

        /**
         * Format the base URL adding a path separator if needed.
         * @param {string} baseUrl The base URL.
         * @returns {string} The formatted base URL.
         */
        public static formatBaseUrl(baseUrl: string): string {
            if (!ObjectExtensions.isNullOrUndefined(baseUrl) && !StringExtensions.isEmptyOrWhitespace(baseUrl)) {
                if (baseUrl.charAt(baseUrl.length - 1) !== UrlHelper.PATH_SEPARATOR) {
                    return baseUrl + UrlHelper.PATH_SEPARATOR;
                }
            }
            return baseUrl;
        }

        /**
         * Checks whether uri is localhost or not.
         * @param {string} uri the URI.
         * @returns {boolean} true if is localhost otherwise false.
         */
        public static isLocalAddress(uri: string): boolean {
            return UrlHelper.LOCAL_ADDRESS_PATTERN.test(uri);
        }

        public static getServerRootUrl(originalUrl: string): string {
            var index: number = originalUrl.toLowerCase().lastIndexOf(UrlHelper.RETAIL_SERVER);

            if (index < 0) {
                return UrlHelper.getRootUrl(originalUrl);
            }

            return originalUrl.substring(0, index + UrlHelper.RETAIL_SERVER.length);
        }

        /**
         * Checks whether the uri has https protocol or not.
         * @param {string} uri the URI.
         * @returns {boolean} true if it is https otherwise false.
         */
        public static isHttpsProtocol(url: string): boolean {
            return StringExtensions.compare(UrlHelper.getProtocol(url), UrlHelper.HTTPS_PROTOCOL, true) === 0;
        }

        /**
         * Get the original root of a URL.
         * Examples:
         * http://localhost:35080/retailserver/commerce will return http://localhost:35080/retailserver
         * http://onecloudbox.dynamics.com:35080/commerce will return http://onecloudbox.dynamics.com:35080
         * Note that on this method, port is assumed as part of URL.
         * @param {string} originalUrl The original URL.
         * @returns {string} The root URL.
         */
        public static getRootUrl(originalUrl: string): string {
            if (StringExtensions.isNullOrWhitespace(originalUrl)) {
                return originalUrl;
            }

            var url: string = originalUrl;
            var rootUrl: string;

            var urlChunks: string[] = url.split(UrlHelper.PATH_SEPARATOR);

            if (url.indexOf(UrlHelper.PROTOCOL_SEPARATOR_WITH_COLON) >= 0) {
                rootUrl = urlChunks[0] + UrlHelper.PROTOCOL_SEPARATOR +  urlChunks[2];
            } else {
                rootUrl = urlChunks[0];
            }

            // Port number is considered part of root URL.
            return rootUrl;
        }

        /**
         * Parses arguments serialized in the URI format defined in IETF RFC3986.
         * @param {argumentString} the serialized arguments.
         * @returns {{ [argumentName: string]: string }} an object containing arguments as members.
         */
        public static parseArguments(argumentString: string): { [argumentName: string]: string } {
            var argumentCollection: { [argumentName: string]: string } = {};
            var argumentPairs: string[] = (argumentString || "").split(UrlHelper.ARGUMENT_SEPARATOR);

            for (var i: number = 0; i < argumentPairs.length; i++) {
                var pairs: string[] = argumentPairs[i].split("=");
                var key: string = decodeURIComponent(pairs[0] || "");
                var value: string = decodeURIComponent(pairs[1] || "");

                if (!StringExtensions.isNullOrWhitespace(key)) {
                    // add both lower case variant to avoid casing issues
                    argumentCollection[key.toLowerCase()] = value;
                    argumentCollection[key] = value;
                }
            }

            return argumentCollection;
        }

        /**
         * Extracts file name from URL.
         * @param {string} url Processing URL.
         * @returns {string} Extracted file name or empty string.
         */
        public static extractFileName(url: string): string {
            // remove hash from url
            var modifiedUrl: string = decodeURI(url);
            var hashStartPos: number = modifiedUrl.indexOf("#");
            if (hashStartPos !== -1) {
                modifiedUrl = modifiedUrl.slice(0, hashStartPos);
            }

            // remove query from url
            var queryStartPos: number = modifiedUrl.indexOf("?");
            if (queryStartPos !== -1) {
                modifiedUrl = modifiedUrl.slice(0, queryStartPos);
            }

            var lastSlashIndex: number = modifiedUrl.lastIndexOf("/");

            if (lastSlashIndex === modifiedUrl.length - 1) {
                return StringExtensions.EMPTY;
            }

            var protocolSeparatorIndex: number = modifiedUrl.lastIndexOf("://");

            // check for protocol
            if ((protocolSeparatorIndex !== -1) && ((protocolSeparatorIndex + 2) === lastSlashIndex)) {
                return StringExtensions.EMPTY;
            }

            // extract file name from end of url
            return modifiedUrl.slice(modifiedUrl.lastIndexOf("/") + 1);
        }

        /**
         * Gets the web-app protocol based on current document.
         * If running under an app container, the web-app protocol will provide access to the web, but not to the underlying app API.
         * @param {Document} document the HTML document.
         * @returns {string} the app protocol.
         */
        private static getWebAppProtocol(document: Document): string {
            var currentProtocol: string = document.location.protocol || "";

            if (UrlHelper.isWWAHost(currentProtocol)) {
                return UrlHelper.WWAHOST_WEBAPP_PROTOCOL;
            } else {
                return currentProtocol;
            }
        }

        /**
         * Gets a value indicating whether the current protocol indicated a WWA host or not.
         * @param {currentProtocol} the current protocol sting.
         * @returns {boolean} a value indicating whether the current protocol indicated a WWA host or not.
         */
        private static isWWAHost(currentProtocol: string): boolean {
            return currentProtocol.indexOf(UrlHelper.WWAHOST_APP_PROTOCOL_NOCOLON) === 0;
        }

        private static getProtocol(url: string): string {
            var anchor: HTMLAnchorElement = document.createElement("a");
            anchor.href = url;
            return anchor.protocol;
        }
    }
}