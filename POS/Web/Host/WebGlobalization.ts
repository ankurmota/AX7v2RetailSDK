/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../SharedApp/Commerce.Core.d.ts'/>
///<reference path='../Pos.web.config.ts' />

module Commerce.Host {
    "use strict";

    interface IWinJSResourceData {
        fileName: string;
        resourceId: string;
        data: string;
    }

    /**
     * Globalization related API for browsers.
     */
    export class WebGlobalization extends Globalization.GlobalizationBase {

        private static WINJS_GLOBAL_STRINGS_KEY: string = "strings"; // NOTE: WinJS uses the key in a global scope for localization

        // This matches javascript's both multiline comments such as "/*....*/" and 
        // single line comments which start with "//" (but won't match URLs such as "https://something.com").
        private static JAVASCRIPT_COMMENT_REGEX: RegExp = /(\/\*([^*] |[\r\n] |(\*+([^*/] |[\r\n])))*\*+\/)|((([^:]\/\/)|(^\/\/)).*)/g;

        private _applicationLanguage: string;

        /**
         * Gets the current application language as IETF tag.
         */
        public getApplicationLanguage(): string {
           return this._applicationLanguage;
        }

        /**
         * Sets the current application language as IETF tag.
         * @param {string} languageTag the new language tag.
         * @return {IVoidAsyncResult} The async result.
         */
        public setApplicationLanguageAsync(languageTag: string): IVoidAsyncResult {
            return this.loadResourcesAsync(languageTag).done((resolvedLanguage: string) => {
                this._applicationLanguage = resolvedLanguage;
            });
        }

        /**
         * Gets the default language as IETF language tag like "en-US".
         * @return {string} The language tag.
         */
        public getDefaultLanguageTag(): string {
            return Config.Web.defaultCultureName;
        }

        /**
         * Initializes an instance.
         * @return {IVoidAsyncResult} The result.
         */
        public initializeAsync(): IVoidAsyncResult {

            var deviceCultureName: string = Commerce.ApplicationContext.Instance && Commerce.ApplicationContext.Instance.deviceConfiguration ?
                Commerce.ApplicationContext.Instance.deviceConfiguration.CultureName : null;

            return this.setApplicationLanguageAsync(deviceCultureName);
        }

        /**
         * Gets the supported languages by the host as IETF language tag like "en-US".
         * @return {string[]} Tags.
         */
        private getSupportedLanguageTags(): string[] {
            return Config.Web.availableCultures;
        }

        /**
         * Loads UI resources asynchronously.
         * @param {string} languageTag the IEFT tag like "en-US".
         * @return {IAsyncResult<string>} the result contains the resolved language which was successfully loaded, or error in case of failure.
         */
        private loadResourcesAsync(languageTag?: string): IAsyncResult<string> {
            languageTag = languageTag || Config.Web.defaultCultureName;
            RetailLogger.applicationGlobalizationResourcesLoading(languageTag);
            languageTag = Globalization.CultureHelper.normalizeLanguageCode(languageTag);
            languageTag = this.resolveAvailableLanguageTag(languageTag);
            RetailLogger.applicationGlobalizationResourcesLanguageResolved(languageTag);

            if (this.areLanguageTagsEqual(languageTag, this._applicationLanguage)) {
                return AsyncResult.createResolved<string>(languageTag);
            }

            var resourcesPath: string = StringExtensions.format(Config.Web.resourcesUriPath, languageTag);

            var resourceFiles: string[] = ArrayExtensions.union(
                [Commerce.Config.Web.primaryResourceFile],
                Commerce.Config.Web.secondaryResources);

            var resourcesData: IWinJSResourceData[] = [];

            var downloadResourcesResults: IAsyncResult<IWinJSResourceData>[] =
                resourceFiles.map((resourceFile: string): IAsyncResult<IWinJSResourceData> => {
                    return DataHelper.loadTextAsync(resourcesPath + resourceFile)
                        .map((data: string): IWinJSResourceData => {
                            return {
                                fileName: resourceFile,
                                resourceId: resourceFile.substr(0, resourceFile.lastIndexOf(".")),
                                data: data
                            };
                        }).done((result: IWinJSResourceData): void => {
                            resourcesData.push(result);
                        });
                });

            var downloadResourcesAsyncResult: IVoidAsyncResult = VoidAsyncResult.join(downloadResourcesResults)
                .done(() => {
                    resourcesData.forEach((value: IWinJSResourceData): void => {
                        this.onResourcesLoadedSuccess(value);
                    });
                }).fail((errors: Model.Entities.Error[]): void => {
                    this.onResourcesLoadedError(errors, languageTag);
                });

            return downloadResourcesAsyncResult.map(() => { return languageTag; });
        }

        private onResourcesLoadedSuccess(resourceData: IWinJSResourceData): void {

            resourceData.data = this.removeJavaScriptComments(resourceData.data);

            var dictionary: any = JSON.parse(resourceData.data);

            if (ObjectExtensions.isNullOrUndefined(dictionary)) {
                RetailLogger.applicationGlobalizationResourcesEmpty();
                return;
            }

            if (ObjectExtensions.isNullOrUndefined(window[WebGlobalization.WINJS_GLOBAL_STRINGS_KEY])) {
                window[WebGlobalization.WINJS_GLOBAL_STRINGS_KEY] = {};
            }

            var resourceDictionary: any = window[WebGlobalization.WINJS_GLOBAL_STRINGS_KEY];

            Object.keys(dictionary).forEach((key: string) => {
                var winJSKey: string = resourceData.fileName !== Commerce.Config.Web.primaryResourceFile ? `/${resourceData.resourceId}/${key}` : key;

                resourceDictionary[winJSKey] = dictionary[key];
            });
        }

        private onResourcesLoadedError(errors: Model.Entities.Error[], languageTag: string): void {
            RetailLogger.applicationGlobalizationResourcesLoadFailed(languageTag, errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
        }

        private removeJavaScriptComments(data: string): string {

            if (StringExtensions.isNullOrWhitespace(data)) {
                return data;
            }

            return data.replace(WebGlobalization.JAVASCRIPT_COMMENT_REGEX, StringExtensions.EMPTY);
        }

        /*
         * @remarks Because the requested language tag may not be available in the application we need to resolve to the closest possible.
         */
        private resolveAvailableLanguageTag(requestedLanguageTag: string): string {

            if (this.isDefaultLanguageTag(requestedLanguageTag)) {
                return requestedLanguageTag;
            }

            var EXACT_MATCH: number = 4; // e.g. 'en-US' matches 'en-US'
            var EXACT_NEUTRAL_MATCH: number = 3; // e.g. 'en-US' matches exactly 'en'
            var NEUTRAL_MATCH: number = 2; // e.g. 'en' matches 'en-US', or 'en-US' matches 'en-GB'
            var DEFAULT_MATCH: number = 1;
            var NO_MATCH: number = 0;

            var requestedNeutralLanguage: string = Globalization.CultureHelper.getLanguageSubTag(requestedLanguageTag);

            var resolvedLanguage: string = Config.Web.defaultCultureName;
            var resolvedLanguageMatch: number = DEFAULT_MATCH;

            var languages: string[] = this.getSupportedLanguageTags();

            for (var i: number = 0; i < languages.length; i++) {
                var language: string = languages[i];
                var languageMatch: number = NO_MATCH;

                if (this.areLanguageTagsEqual(language, requestedLanguageTag)) {
                    languageMatch = EXACT_MATCH;
                } else {
                    var neutralLanguage: string = Globalization.CultureHelper.getLanguageSubTag(language);
                    if (this.areLanguageTagsEqual(neutralLanguage, requestedNeutralLanguage)) {
                        if (this.areLanguageTagsEqual(neutralLanguage, language)) {
                            languageMatch = EXACT_NEUTRAL_MATCH;
                        } else {
                            languageMatch = NEUTRAL_MATCH;
                        }
                    }
                }

                // if we found a same or better match update the resolved language
                if (languageMatch >= resolvedLanguageMatch) {
                    resolvedLanguageMatch = languageMatch;
                    resolvedLanguage = language;
                }

                // if this is the highest possible match then exit
                if (resolvedLanguageMatch === EXACT_MATCH) {
                    break;
                }
            }

            return resolvedLanguage;
        }

        private isDefaultLanguageTag(languageTag: string): boolean {
            return this.areLanguageTagsEqual(languageTag, Config.Web.defaultCultureName);
        }

        private areLanguageTagsEqual(languageTag1: string, languageTag2: string): boolean {
            return (StringExtensions.compare(languageTag1, languageTag2, /*ignoreCase:*/ true) === 0);
        }
    }
}
