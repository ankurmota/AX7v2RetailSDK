/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Config.Web {

    // WebPOS globalization configuration settings.

    /**
     * List of available application localization names in IETF format.
     * @remarks Corresponds with the files, defined in Commerce.Config.Web.Globalization.resourcesUriPath field.
     * @remarks When adding/removing resources (*.resjson) to application, availableCultures should be modified accordingly.
     * @todo Add this requirement to the WebPOS documentation.
     */
    export var availableCultures: string[] = [
        "ar", "cs", "da", "de", "de-AT", "de-CH", "en-AU", "en-CA", "en-GB", "en-IE", "en-IN",
        "en-MY", "en-NZ", "en-SG", "en-US", "en-ZA", "es", "es-MX", "et", "fi", "fr", "fr-BE",
        "fr-CA", "fr-CH", "hu", "is", "it", "it-CH", "ja", "lt", "lv", "nb-NO", "nl", "nl-BE",
        "pl", "pt-BR", "ru", "sv", "th", "tr", "zh-Hans"
    ];

    /**
     * Default culture, used if specific one is not configured nor available.
     */
    export var defaultCultureName: string = "en-US";

    /**
     * Default currency code as (ISO 4217).
     */
    export var defaultCurrencyCode: string = "USD";

    /**
     * Path to the resources files.
     * @remarks {0} - IETF language tag
     */
    export var resourcesUriPath: string = "Assets/Strings/{0}/";

    /**
     * Primary resource file name.
     */
    export var primaryResourceFile: string = "resources.resjson";

    /**
     * List of secondary resource files.
     */
    export var secondaryResources: string[] = [ "languages.resjson", "territories.resjson" ];
}