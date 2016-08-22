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

    /**
     *  This class defines the application custom WinJS bindings.
     *  All methods in the class will be exposed as WinJS bindings and can be accessed as: Commerce.CustomBindings.[methodName], for example, Commerce.CustomBindings.SetDefaultImage.
     */
    class CustomWinJSBindings {
        /**
         *  Sets the default image for an img element on error. Usage example: data-win-bind="onerror: Commerce.DefaultImages.ProductSmall Commerce.DefaultImages.CustomerLarge".
         *  @param {any} source The source binding element.
         *  @param {string[]} sourceProperty The source binding element property path.
         *  @param {HTMLElement} dest The destination element being bound.
         *  @param {string[]} destProperty The destination binding element property path.
         */
        public static SetDefaultImage(source: any, sourceProperty: string[], dest: HTMLElement, destProperty: string[]): void {
            if (destProperty.length !== 1 || destProperty[0] !== "onerror") {
                throw new Error("Only 'onerror' destination property is supported for binding 'SetDefaultImage'. Provided value was: '" + destProperty + "'.");
            }

            dest.addEventListener("error", () => {
                // winjs does not evaluate bindings, like knockout, we need to get the value for the property
                if (ArrayExtensions.hasElements(sourceProperty) && sourceProperty[0] === "Commerce") {
                    // looking up a value in Commerce module
                    var value: any = Commerce;
                    for (var i = 1; i < sourceProperty.length && value != null; i++) {
                        value = value[sourceProperty[i]];
                    }

                    var url: string = value || "";
                    BindingHandlers.SetDefaultImageOnError(dest, url);
                } else {
                    // not supported
                    RetailLogger.genericError("Commerce.CustomBindings.SetDefaultImage was called with invalid image path: " + (sourceProperty || []).join());
                }
            });
        }
    }

    // Register custom bindings with WinJS
    (function () {
        WinJS.Namespace.define("Commerce.CustomBindings", {
            "SetDefaultImage": WinJS.Binding.initializer(CustomWinJSBindings.SetDefaultImage)
        });
    })();
}