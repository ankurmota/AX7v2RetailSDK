/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../SharedApp/Commerce.Core.d.ts'/>

module Commerce.Host {
    "use strict";

    /**
     * WinRT implementation of Globalization related API.
     */
    export class WwaGlobalization extends Globalization.GlobalizationBase {

        /**
         * Gets the current application langauge as IETF tag.
         */
        public getApplicationLanguage(): string {
            return Windows.Globalization.ApplicationLanguages.primaryLanguageOverride;
        }

        /**
         * Sets the current application language as IETF tag.
         * @param {string} languageTag the new language tag.
         * @return {IVoidAsyncResult} The async result.
         */
        public setApplicationLanguageAsync(languageTag: string): IVoidAsyncResult {
            Windows.Globalization.ApplicationLanguages.primaryLanguageOverride = languageTag;

            return VoidAsyncResult.createResolved();
        }

        /**
         * Gets the default language as IETF language tag like "en-US".
         * @return {string} The language tag.
         */
        public getDefaultLanguageTag(): string {
            return this.getSupportedLanguageTags()[0];
        }

        /**
         * Gets supported languages by the host as short tag like "en-US".
         * @return {string[]} Tags.
         */
        private getSupportedLanguageTags(): string[] {
            var manifestLanguages: Windows.Foundation.Collections.IVectorView<string> = Windows.Globalization.ApplicationLanguages.manifestLanguages;
            return manifestLanguages.slice(0);
        }
    }
}