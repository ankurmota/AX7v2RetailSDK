/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Contoso.Retail.Ecommerce.Sdk.Controls {
    "use strict";
    
    export var ResourceStrings = {};
    export var Resources: any = {};

    export class ResourcesHandler {

		public static selectUICulture() {
            var uiCultureFromCookie = Utils.getCurrentUiCulture();
            // If the resources for the given culture exist then we use them otherwise
            // we fall back to the en-us resources.
            if (ResourceStrings[uiCultureFromCookie]) {
                Resources = ResourceStrings[uiCultureFromCookie];
            }
            else {
                Resources = ResourceStrings["en-us"];
            }
        }
	}
}
