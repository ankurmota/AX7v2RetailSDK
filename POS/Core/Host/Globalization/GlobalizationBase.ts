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
     * Globalization related API.
     */
    export class GlobalizationBase implements IGlobalization {

        /**
         * When implemented in child class, gets the current application language as IETF tag.
         */
        public getApplicationLanguage(): string {
            throw "Abstract method. Not implemented";
        }

        /**
         * When implemented in child class, sets the current application language as IETF tag.
         * @param {string} the new language tag.
         * @return {IVoidAsyncResult} The async result.
         */
        public setApplicationLanguageAsync(languageTag: string): IVoidAsyncResult {
            throw "Abstract method. Not implemented";
        }

        /**
         * When implemented in child class, gets the default language as IETF language tag like "en-US".
         * @return {string} The language tag.
         */
        public getDefaultLanguageTag(): string {
            throw "Abstract method. Not implemented";
        }

        /**
         * Gets detailed information about the language by its tag.
         * @param {string} languageTag A tag like "en-US".
         * @return {ILanguage} Language information.
         */
        public getLanguageByTag(languageTag: string): ILanguage {
            var displayName: string = Commerce.Host.Globalization.CultureHelper.getLanguageByCode(languageTag);

            return { displayName: displayName };
        }

        /**
         * Gets the formatter for the specific format.
         * @param {Host.DateTimeFormat} format A format template that specifies the requested components.
         * @return {IDateTimeFormatter} The formatter.
         */
        public getDateTimeFormatter(format: string): IDateTimeFormatter {
            var currentChannelTimeZone: string;
            if (ApplicationContext.Instance && ApplicationContext.Instance.channelConfiguration) {
                currentChannelTimeZone = ApplicationContext.Instance.channelConfiguration.TimeZoneInfoId;
            }

            var cultureName: string = ArrayExtensions.firstOrUndefined(
                [this.getStoreLanguage(), this.getCompanyLanguage(), this.getApplicationLanguage()],
                (s: string) => !StringExtensions.isNullOrWhitespace(s));

            return new Globalization.TimeZoneDateTimeFormatter(format, cultureName, currentChannelTimeZone);
        }

        /**
         * Gets the company language, if specified.
         *
         * @return {string} The company language.
         */
        private getCompanyLanguage(): string {
            var language: string;
            if (!ObjectExtensions.isNullOrUndefined(ApplicationContext.Instance)
                && !ObjectExtensions.isNullOrUndefined(ApplicationContext.Instance.channelConfiguration)) {
                language = ApplicationContext.Instance.channelConfiguration.DefaultLanguageId;
            }

            return language;
        }

        /**
         * Gets the store language, if specified.
         *
         * @return {string} The store language.
         */
        private getStoreLanguage(): string {
            var language: string;
            if (!ObjectExtensions.isNullOrUndefined(ApplicationContext.Instance)
                && !ObjectExtensions.isNullOrUndefined(ApplicationContext.Instance.deviceConfiguration)) {
                language = ApplicationContext.Instance.deviceConfiguration.CultureName;
            }

            return language;
        }
    }
}