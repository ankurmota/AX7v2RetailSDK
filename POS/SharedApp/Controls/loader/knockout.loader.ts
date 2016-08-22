/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.Core.d.ts'/>

module Commerce.Controls {
    "use strict";

    /*
     * Loader types.
     */
    export enum LoaderType {
        Page = 1,
        Nested = 2,
        Dialog = 3
    }

    export enum LoaderColor {
        White = 1,
        Accent = 2
    }

    /*
     * Available options for loader
     */
    export interface ILoaderOptions {
        visible?: Observable<boolean>;
        type?: LoaderType;
        isSmall?: boolean;
        color?: LoaderColor;
    }

    /**
     * custom loader binding.
     * usage example <div data-bind="loader: { visible: {Observable<boolean>}, type: {LoaderType} }"></div>
     * default {LoaderType} is page loader. It means that it will take the whole screen and will block it.
     */
    class Loader implements KnockoutBindingHandler {

        /**
         * defines template name used for loader depending on current browser.
         */
        private static pageLoaderClass: string = "page-loader";
        private static nestedLoaderClass: string = "nested-loader";
        private static dialogLoaderClass: string = "dialog-loader";
        private static templateName: string = "loader-template";
        private static innerCircleClassFormat: string = "inner-circle {0}";
        private static colors: Array<string> = ["inner-circle-white", "accentBackground"];

       /** 
        * Toggles element visibility state.
        * @param {JQuery} $element: JQuery element for switching visibility .
        * @param {ILoaderOptions} options: Binding options.
        */
        private static toggleElementVisibility($element: JQuery, options: ILoaderOptions): void {
            var isVisible: boolean = $element.css("display") !== "none";
            var optionVisible: boolean = ko.utils.unwrapObservable(options.visible) || false;

            if (optionVisible !== isVisible) {
                $element.toggle();
            }
        }

        /*
         * Implementation of {KnockoutBindingHandler} interface method
         */
        public init(element: HTMLElement, valueAccessor: () => any, allBindingsAccessor: () => any,
            viewModel: any, bindingContext: KnockoutBindingContext): any {
            var options: ILoaderOptions = ko.utils.unwrapObservable(valueAccessor() || {});
            options.color = options.color || LoaderColor.Accent;
            options.type = options.type || LoaderType.Page;
            options.isSmall = ObjectExtensions.isNullOrUndefined(options.isSmall) ? false : options.isSmall;
            var $element: JQuery = $(element);
            var data: any = {
                isSmall: options.isSmall,
                innerCircleClass: StringExtensions.format(Loader.innerCircleClassFormat, Loader.colors[options.color - 1])
            };
            var loaderClass: string;
            var template: any = {
                name: Loader.templateName,
                data: data
            };

            ko.applyBindingsToNode(element, { template: template });

            switch (options.type) {
                case LoaderType.Page:
                    loaderClass = Loader.pageLoaderClass;
                    break;
                case LoaderType.Nested:
                    loaderClass = Loader.nestedLoaderClass;
                    $element.parent().css("position", "relative");
                    break;
                case LoaderType.Dialog:
                    loaderClass = Loader.dialogLoaderClass;
                    break;
                default:
                    throw new Error(StringExtensions.format("Unknown loader type {0}", options.type));
            }

            $element.addClass(loaderClass);
            Loader.toggleElementVisibility($element, options);

            return { controlsDescendantBindings: true };
        }

        /*
         * Implementation of {KnockoutBindingHandler} interface method
         */
        public update(element: HTMLElement, valueAccessor: () => any, allBindingsAccessor: () => any,
            viewModel: any, bindingContext: KnockoutBindingContext): any {

            var $element: JQuery = $(element);
            var options: ILoaderOptions = ko.utils.unwrapObservable(valueAccessor() || {});

            Loader.toggleElementVisibility($element, options);

            return { controlsDescendantBindings: true };
        }
    }

    ko.bindingHandlers.loader = new Loader();

}

