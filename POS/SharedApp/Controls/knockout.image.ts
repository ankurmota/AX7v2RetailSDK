/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../KnockoutJS.d.ts'/>
///<reference path='../JQuery.d.ts' />

/*
 *  knockout image
 *
 *
 * USAGE EXAMPLE:
 * image: { 
 *          src: "/path/to/image.png",
 *          defaultSrc: "/default/image/onerror.png"
 *        }
 *
 * @param {string} src (Required) Specifies the source string to attempt to load.
 * @param {string} defaultSrc (Required) Specifies the source string to load if there is an error while trying to load the initial "src".
 *
 * HTML MARKUP:
 *     <img src="#" data-bind="image: { src: '/path/to/image.png', defaultSrc: Commerce.DefaultImages.CustomerSmall }" />  
 *
 */

ko.bindingHandlers.image = (() => {
    "use strict";

    return {
        init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
            $(element).on("error", () => {
                var value = valueAccessor(),
                    defaultSrc = value.defaultSrc;
                Commerce.BindingHandlers.SetDefaultImageOnError(element, defaultSrc);
            });
        },
        update: function (element, valueAccessor) {
            var value = valueAccessor(),
                src = value.src,
                defaultSrc = value.defaultSrc;

            if (!Commerce.StringExtensions.isNullOrWhitespace(src)) {
                $(element).attr("src", src);
            } else {
                $(element).attr("src", defaultSrc);
            }
        }
    };
})(); 