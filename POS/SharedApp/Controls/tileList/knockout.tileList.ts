/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.Core.d.ts'/>

module Commerce {
    export module TileList {
        export interface ITileField {
            field: string;
            converter?: string;
            cssClass?: string;
            cssClassField?: string;
        }

        export interface ITileListOptions {
            itemDataSource: any;
            afterRender: any; // used for internal purpose. 
            iteminvoked?: (eventArgs: IItemInvokedArgs) => void;
            center?: ITileField;
            bottomLeft?: ITileField;
            width?: number; // Width of the block in ems
            height?: number; // Height of the block in ems
            flipInPortrait?: boolean; // determines whether the tile list will flip in portrait view (go from column to row)
        }

        export interface IItemInvokedArgs {
            data: any;
        }
    }
}

/*
    tileList knockout binding extension
*/
/*
* tileList binding usage example:
* tileList: {            
                itemDataSource: dataArray,
                oninvoked: function (eventArgs) {
                    // var data = eventArgs.data;
                },
                bottomLeft: {
                    field: 'dataField',
                    converter: 'Commerce.Formatters.SomeFormatter',
                    cssClass: 'customClass',
                    cssClassField: 'dataCssClass'
                } 
                center: {
                    field: 'dataField',
                    converter: 'Commerce.Formatters.SomeFormatter',
                    cssClass: 'customClass',
                    cssClassField: 'dataCssClass'
                }
            }
* */
ko.bindingHandlers.tileList = {
    init(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value: Commerce.TileList.ITileListOptions = ko.utils.unwrapObservable(valueAccessor()) || {};
        var $element = $(element);
        var defaultSize: number = 12;
        var sizeFormat: string = "{0}em";
        var baseWidth: number = value.width || defaultSize;
        var baseHeight: number = value.height || defaultSize;

        $element.addClass("tileList col wrap");

        // set defaults on all properties, so knockout would not have troubles binding.
        var updateFieldValues = (tileField: Commerce.TileList.ITileField, defaultCssClass: string) => {
            if (tileField) {
                tileField.field = tileField.field || "";
                tileField.converter = tileField.converter || "";
                tileField.cssClass = tileField.cssClass || defaultCssClass;
                tileField.cssClassField = tileField.cssClassField || "";
            }
        };

        value.bottomLeft = value.bottomLeft || null;
        updateFieldValues(value.bottomLeft, "");
        value.center = value.center || null;
        updateFieldValues(value.center, ""); //used for icon. 

        // Build tile styles
        var blockStyle: any = {
            "min-width": Commerce.StringExtensions.format(sizeFormat, baseWidth),
            "max-width": Commerce.StringExtensions.format(sizeFormat, baseWidth),
            "min-height": Commerce.StringExtensions.format(sizeFormat, baseHeight),
            "max-height": Commerce.StringExtensions.format(sizeFormat, baseHeight)
        }
        var afterRenderHandler = () => {
            var viewModelTemp = viewModel; // to ensure we are not loosing viewModel in the context switching.

            // Parse each container and update elements that need additional options.
            $element.find(".itemContainer").each((index, itemContainerElement) => {
                // Do not bind if element already have winControl
                if (!itemContainerElement.winControl) {
                    var rowData = value.itemDataSource()[index];

                    // Using this below to act as a container (<any>itemContainerElement.parentNode).defaultFocusSet
                    // without doing that I will lose variable scope.

                    var $itemContainer = $(itemContainerElement);
                    if ((((<any>itemContainerElement.parentNode).defaultFocusSet !== true) || index === 0)
                        && !rowData.disabled) {
                        $itemContainer.focus();
                        (<any>itemContainerElement.parentNode).defaultFocusSet = true;
                    }
                    else if (rowData.disabled) {
                        (<any>itemContainerElement).tabIndex = -1;
                    }

                    // Since string not supported to set size on the element using knockout we are setting the style using JQuery attribute.
                    $itemContainer.css(blockStyle);

                    // Create ItemContainer element
                    var itemContainerOptions = {
                        swipeBehavior: WinJS.UI.SelectionMode.none,
                        selectionDisabled: true,
                        tapBehavior: WinJS.UI.TapBehavior.invokeOnly
                    };
                    var itemContainer = new WinJS.UI.ItemContainer(<HTMLElement>itemContainerElement, itemContainerOptions);
                    $itemContainer.addClass('highContrastBorder');

                    if (!rowData.disabled) {
                        // Attach oninvoked handler
                        var itemInvokeHandler = (event: Event) => {
                            if (value.iteminvoked) {
                                value.iteminvoked.call(viewModelTemp, <Commerce.TileList.IItemInvokedArgs>{ data: rowData });
                            }
                            event.stopPropagation();
                        };
                        var handler = WinJS.Utilities.markSupportedForProcessing(itemInvokeHandler);
                        itemContainer.addEventListener("invoked", handler);
                    }

                    // Add 'col' class to elements that come from WinJS control to ensure content occupies full height.
                    $itemContainer.find(".win-itembox, .win-item").addClass("col grow");
                }
            });
        };

        value.afterRender = afterRenderHandler;

        // render template
        ko.applyBindingsToNode(element, {
            template: {
                name: 'tileListTemplate',
                data: value
            }
        }, this);

        if (value.flipInPortrait) {
            $element.find(".itemList").addClass("flip");
        }

        return { controlsDescendantBindings: true };
    }
};
