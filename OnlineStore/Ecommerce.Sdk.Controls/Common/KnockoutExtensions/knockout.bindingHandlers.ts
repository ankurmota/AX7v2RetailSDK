/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../../KnockoutJS.d.ts" />

module Contoso.Retail.Ecommerce.Sdk.Controls {
     /*
     * Resource knockout binding extension.
     *
     *
     * Resource binding usage example:
     * resx: { textContent: 'someString' }
     *
     * @param {string} [textContent] - The textContent attribute of html element.
     * @param {string} [label] - The label attribute of html element.
     *
     */
    export interface IResx {
        textContent?: string;
        label?: string;
    }
}

ko.bindingHandlers.resx = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // This will be called when the binding is first applied to an element
        // Set up any initial state, event handlers, etc. here

        var binding: Contoso.Retail.Ecommerce.Sdk.Controls.IResx = ko.utils.unwrapObservable(valueAccessor()) || {};

        for (var memberName in binding) {
            switch (memberName) {
                case "textContent":
                    // The resource string is associated to the textContent attribute of the element.
                    element.textContent = Contoso.Retail.Ecommerce.Sdk.Controls.Resources[binding[memberName]];
                    break;

                case "label":
                    // The resource string is associated to the label attribute of the element.
                    element.label = Contoso.Retail.Ecommerce.Sdk.Controls.Resources[binding[memberName]];
                    break;
            }
        }
    }
};