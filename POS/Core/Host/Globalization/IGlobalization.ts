/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Host {
    "use strict";

    /**
     * Globalization related API.
     */
    export interface IGlobalization {
        /**
         * Gets the current application language as IETF tag.
         */
        getApplicationLanguage(): string;

        /**
         * Sets the current application language as IETF tag.
         * @param {string} the new language tag.
         * @return {IVoidAsyncResult} The async result.
         */
        setApplicationLanguageAsync(languageTag: string): IVoidAsyncResult;

        /**
         * Gets the default language as IETF language tag like "en-US".
         * @return {string} The language tag.
         */
        getDefaultLanguageTag(): string;

        /**
         * Gets detailed information about the language by its tag.
         * @param {string} languageTag A tag like "en-US".
         * @return {ILanguage} Language information.
         */
        getLanguageByTag(languageTag: string): ILanguage;

        /**
         * Gets the formatter for the specific format.
         * @param {string} format See Commerce.Host.DateTimeFormat for predefined formats.
         * @return {IDateTimeFormatter} The formatter.
         */
        getDateTimeFormatter(format: string): IDateTimeFormatter;
    }
}