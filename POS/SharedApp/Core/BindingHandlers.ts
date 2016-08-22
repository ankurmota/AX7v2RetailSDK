/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Commerce.Core.d.ts'/>

module Commerce {
    "use strict";

    export class DefaultImages {
        
        public static ProductSmall: string = "Assets/defaultSmall.png";
        public static ProductLarge: string = "Assets/defaultLarge.png";
        public static CustomerLarge: string = "Assets/person_unavailable_large.png";
        public static CustomerSmall: string = "Assets/person_unavailable_small.png";
        public static DefaultUser: string = "Assets/DefaultUser.png";
        public static AllProducts: string = "Assets/fabrikam_all_products.jpg";
        
    }

    export class BindingHandlers {

        /**
         *   Sets the source of an element to the default image.
         *   @param {any} element Element to set attributes.
         *   @param {string} imageUrl Identifier of the image to display.
         */
        public static SetDefaultImageOnError(element: any, imageUrl: string): void {
            var $element: JQuery = $(element);
            var originalSrc: string = $element.attr("src");
            RetailLogger.coreBindingHandlersLoadImageFailed(originalSrc);

            var isIEBrowserOrMpos: boolean = ((Host.instance.application.getApplicationType() === Proxy.Entities.ApplicationTypeEnum.CloudPos) &&
                (Host.instance.application.getBrowserType() === Host.BrowserType.IE11)) ||
                (Host.instance.application.getApplicationType() === Proxy.Entities.ApplicationTypeEnum.MposForWindows);

            if (isIEBrowserOrMpos) {
                // NOTE: this is a specific workaround for IE and mpos
                // because it tends to throw a false positive on error
                // The workaround is required until we find a root cause.
                var image: HTMLImageElement = document.createElement("img");
                image.onerror = () => {
                    $element.attr("src", imageUrl);
                };
                image.src = originalSrc;
            } else {
                $element.attr("src", imageUrl);
            }
       }
    }
}