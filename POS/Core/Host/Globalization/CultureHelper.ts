/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Host.Globalization {
    "use strict";

    /**
     * Helper class for Culture-related operations.
     */
    export class CultureHelper {

        // NOTE: from 2 to 8 Latin letters, also either EOL is expected or hyphen with additional subtags
        private static LANGUAGE_CODE_REGEX: RegExp = /^([a-zA-Z]{2,8})(?:-|$)/i;
        private static LANGUAGE_SCRIPT_REGEX: RegExp = /^([a-zA-Z]{2,8}-)([a-zA-Z]{4})(-.+|$)/i;
        private static REGION_CODE_REGEX: RegExp = /-([a-zA-Z]{2}|\d{3})(?:[-]|$)/i;
        private static DEFAULT_COMPLEX_LANGUAGE_TEMPLATE: string = "{0} ({1})"; // Example: "English (United States)".
        private static SUBTAG_REGEX: RegExp = /[-]/g;
        private static REGION_RESOURCE_ID: string = "/territories/";
        private static LANGUAGE_RESOURCE_ID: string = "/languages/";

        /**
         * Gets the language name by it's IETF code
         * @param {string} languageCode The IETF language code.
         * @return {string} The language name or language code if language not found, or empty string if culture code undefined, null or empty string.
         */
        public static getLanguageByCode(languageCode: string): string {

            if (StringExtensions.isNullOrWhitespace(languageCode)) {
                return StringExtensions.EMPTY;
            }

            languageCode = CultureHelper.normalizeLanguageCode(languageCode);

            var languageResourceKey: string = CultureHelper.LANGUAGE_RESOURCE_ID + languageCode;
            var languageName: string = ViewModelAdapter.getResourceString(languageResourceKey);
            var isExactLanguageNameFound: boolean = !StringExtensions.isNullOrWhitespace(languageName) && (languageResourceKey !== languageName);

            if (isExactLanguageNameFound) {
                // We've found an exact language name in languages.resjson (i.e. zh-Hans = "Simplified Chineese", or en = "English").
                return languageName;
            } else {
                // Else constructing language name as a concatenation of language and region (i.e. en-US - "English (United States)").

                var languageSubTag: string = CultureHelper.getLanguageSubTag(languageCode);
                var regionSubTag: string = CultureHelper.getRegionSubTag(languageCode);
                languageResourceKey = CultureHelper.LANGUAGE_RESOURCE_ID + languageSubTag;
                languageName = ViewModelAdapter.getResourceString(languageResourceKey);
                var isLanguageNameFound: boolean = !StringExtensions.isNullOrWhitespace(languageName) && (languageResourceKey !== languageName);

                var regionResourceKey: string = CultureHelper.REGION_RESOURCE_ID + regionSubTag;
                var regionName: string = ViewModelAdapter.getResourceString(regionResourceKey);
                var isRegionNameFound: boolean = !StringExtensions.isNullOrWhitespace(regionName) && (regionResourceKey !== regionName);

                if (isLanguageNameFound && isRegionNameFound) {
                    return StringExtensions.format(CultureHelper.DEFAULT_COMPLEX_LANGUAGE_TEMPLATE, languageName, regionName);
                } else {
                    return languageCode;
                }
            }
        }

        /**
         * Gets the normalized language IETF code.
         * @param {string} languageCode The IETF language code.
         * @return {string} The normalized language IETF code or empty string, if culture code is not IETF-compliant.
         */
        public static normalizeLanguageCode(languageCode: string): string {
            if (StringExtensions.isNullOrWhitespace(languageCode)) {
                return StringExtensions.EMPTY;
            }

            return languageCode
                .replace(CultureHelper.LANGUAGE_CODE_REGEX, (s: string): string => { return s.toLowerCase(); })
                .replace(CultureHelper.LANGUAGE_SCRIPT_REGEX, (s: string, language: string, script: string, rest: string): string => {
                     return language + script.charAt(0).toUpperCase() + script.slice(1).toLowerCase() + rest;
                })
                .replace(CultureHelper.REGION_CODE_REGEX, (s: string): string => { return s.toUpperCase(); });
        }

        /**
         * Gets the language code of the given IETF culture code.
         * @param {string} cultureCode The IETF culture code.
         * @return {string} The language IETF code or empty string, if culture code is not IETF-compliant.
         */
        public static getLanguageSubTag(cultureCode: string): string {
            if (StringExtensions.isNullOrWhitespace(cultureCode)) {
                return StringExtensions.EMPTY;
            }

            var result: string[] = cultureCode.match(CultureHelper.LANGUAGE_CODE_REGEX);
            return ArrayExtensions.hasElements(result) ?
                result[0].replace(CultureHelper.SUBTAG_REGEX, StringExtensions.EMPTY) :
                StringExtensions.EMPTY;
        }

        /**
         * Gets the region code of the given IETF culture code.
         * @param {string} cultureCode The IETF culture code.
         * @return {string} The region IETF code or empty string, if culture code is not IETF-compliant.
         */
        public static getRegionSubTag(cultureCode: string): string {
            if (StringExtensions.isNullOrWhitespace(cultureCode)) {
                return StringExtensions.EMPTY;
            }

            var result: string[] = cultureCode.match(CultureHelper.REGION_CODE_REGEX);
            return ArrayExtensions.hasElements(result) ?
                result[0].replace(CultureHelper.SUBTAG_REGEX, StringExtensions.EMPTY) :
                StringExtensions.EMPTY;
        }
    }
}