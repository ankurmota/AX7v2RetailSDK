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

    export module ListView {
        export class FieldType {
            public static html = "html";
            public static text = "text";
        }

        export interface IListViewOptionColumn {
            titleResx?: string;
            title?: string;
            cssClass?: string;
            field: string;
            converter?: string;
            type?: string; // "text" || "html" only available in knockout list view
            defaultValueResx?: string;
            defaultValue?: string;
            lines?: {
                field: string;
                type?: string; // "text" || "html" only available in knockout list view
                converter?: string;
            }[];
        }

        export interface IListViewOptions {
            useWinJSListView?: boolean;
            selectionMode?: WinJS.UI.SelectionMode;
            tapBehavior?: string;
            emptyListViewTemplate?: string;
            expandable?: boolean; //only available in knockout list view
            expandableRow?: string; //only available in knockout list view
            expandableRowConverter?: () => boolean; //only available in knockout list view
            rowClass?: string; //only available in knockout list view
            headerRowClass?: string; //only available in knockout list view
            rowTemplate?: string; // only available in knockout list view.
            headerRowTemplate?: string // only available in knockout list view.
            strikethroughRowConverter?: () => boolean; //only available in knockout list view
            strikethroughRow?: string; //only available in knockout list view
            strikethroughEnabled?: boolean;
            rowExpanded?: (eventArgs: IItemExpand) => void; // only available in knockout list view
            incrementalDataSource?: any;
            itemDataSource?: any;
            iteminvoked?: (eventArgs: IItemInvoked) => void;
            dataRowsAfterRender?: any; // internal var
            scrollToBottom?: boolean; // scroll
            selectionchanged?: {
                eventHandlerCallBack: (items: any[]) => void;
                appBarId: string;
            };

            forceControlLayout?: any; //function which allows you to force layout of the grid
            columns?: Commerce.ListView.IListViewOptionColumn[];
        }

        export interface IItemExpand {
            colspanRow: Element;
            currentTarget: Element;
            data: any;
        }

        export interface IItemInvoked {
            currentTarget: Element;
            data: any;
        }
    }

    export interface ITextConverterOptions {
        data: any;
        name: string;
        type: string; // "text" || "html"
    }
}

/* bind text using converter */
/* * usage example:
* textConverter: { data: $data, field: 'someField', converter: 'Commerce.Converter.Converter', type: 'html', defaultValue: '0' }    */
ko.bindingHandlers.textConverter = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value = ko.utils.unwrapObservable(valueAccessor()) || {};
        var data = value.data;
        var fieldValue;
        var convertedValue;

        if (value.field === "$data") {
            fieldValue = data;
        } else {
            fieldValue = data[value.field];
        }

        if (!Commerce.StringExtensions.isNullOrWhitespace(value.converter)) {
            var pathNames = value.converter.split('.');
            var converterMethod: any = <any>window[pathNames[0]]; // Initialize with first level. Example 'Commerce' 

            // Starting from next element drill down to the method. 
            for (var i = 1; i < pathNames.length; i++) {
                converterMethod = converterMethod[pathNames[i]];
            }

            if (converterMethod.supportedForProcessing) {
                // Process WinJS converter.
                var destinationData = {
                    convertedValue: "",
                    winBindingToken: { configurable: false, writable: false, enumerable: false, value: "innerText" }
                };
                var sourceProperties = [];
                var destinationProperties = ["convertedValue"];
                if (Commerce.ObjectExtensions.isNullOrUndefined(fieldValue)) {
                    if (!Commerce.ObjectExtensions.isNullOrUndefined(value.defaultValue)) {
                        converterMethod(value.defaultValue, sourceProperties, destinationData, destinationProperties, null);
                        convertedValue = destinationData.convertedValue;
                    } else {
                        convertedValue = "";
                    }
                } else {
                    converterMethod(fieldValue, sourceProperties, destinationData, destinationProperties, null);
                    convertedValue = destinationData.convertedValue;
                }
            } else {
                // Execute regular converter method
                convertedValue = converterMethod(fieldValue);
            }
        } else {
            if (Commerce.ObjectExtensions.isNullOrUndefined(fieldValue)) {
                if (!Commerce.ObjectExtensions.isNullOrUndefined(value.defaultValue)) {
                    convertedValue = value.defaultValue;
                } else {
                    convertedValue = "";
                }
            } else {
                convertedValue = fieldValue;
            }
        }

        var type = value.type ? value.type : Commerce.ListView.FieldType.text;
        var binding = {};
        binding[type] = convertedValue;

        if (value.cssClassField) {
            binding['css'] = data[value.cssClassField];
        }

        ko.applyBindingsToNode(element, binding);
    }
};

/*
    listView knockout binding extension
*/
/*
* listView binding usage example:
* listView: {            
                itemDataSource: dataArray,
                selectionMode: WinJS.UI.SelectionMode.single,
                expandable: true,
                expandableRow: 'expandableRowField', [expandableConverter: $root.someMethod,]
                onexpand: function (eventArgs) {
                    $(eventArgs.colspanRow).html("<div>something in colspan</div>");
                },
                oninvoked: function (eventArgs) {
                    // var data = eventArgs.data
                },
                onselectionchanged: function (eventArgs) {

                },
                strikethroughRow: "strikethroughField",
                columns: [
                    {
                        title: "Col1",
                        cssClass: "ratio1",
                        field: "title",
                        type: "text",
                        converter: "Commerce.Core.Converter.SomeFormatter",
                        line1: { field: "title", type: "html", converter: "Commerce.Core.Converter.SomeFormatter" },
                        line2: { field: "picture", converter: "" }
                    },
                    {
                        title: "Col2",
                        cssClass: "ratio9",
                        field: "firstName",
                        converter: "",
                        line2: { field: "lastName", converter: "" }
                    }]
            }


* */
ko.bindingHandlers.listView = {
    init(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value: Commerce.ListView.IListViewOptions = ko.utils.unwrapObservable(valueAccessor()) || {};
        var $element = $(element);
        var listViewControl: any;
        var forceLayoutDone = "forceLayoutDone";

        // Removes knockout bindings on template.
        var cleanKnockoutBinding = elementToClean => {
            $element.find("[data-bind]").each((index, el) => {
                $(el).removeAttr("data-bind");
            });
        };

        value.emptyListViewTemplate = value.emptyListViewTemplate || "emptyListViewTemplate";
        value.columns = value.columns || [];
        value.rowTemplate = value.rowTemplate || Commerce.StringExtensions.EMPTY;
        value.headerRowTemplate = value.headerRowTemplate || Commerce.StringExtensions.EMPTY;

        if (value.selectionMode === WinJS.UI.SelectionMode.none) {
            $element.addClass("no-select");
        }
        // StandardGrid
        if (Commerce.ArrayExtensions.hasElements(value.columns) ||
            (!Commerce.StringExtensions.isNullOrWhitespace(value.rowTemplate) && !Commerce.StringExtensions.isNullOrWhitespace(value.headerRowTemplate))) {

            // Set values for optional fields
            value.expandable = value.expandable || false;
            value.strikethroughEnabled = value.strikethroughEnabled || false;
            value.selectionMode = value.selectionMode || WinJS.UI.SelectionMode.multi;
            value.rowClass = value.rowClass || "";
            value.headerRowClass = value.headerRowClass || "";

            // If expandable functionality not needed then we will user WinJS ListView.
            // WinJS grid was used to implemenet first row selection and incremental load which is not available with knockout implementation yet.
            // linethrough only available in knockout list view. 
            var useWinJSListView = (!value.expandable && !value.strikethroughEnabled);
            if (!Commerce.ObjectExtensions.isNullOrUndefined(value.useWinJSListView)) {
                useWinJSListView = value.useWinJSListView;
            }

            if (Commerce.ArrayExtensions.hasElements(value.columns)) {
                for (var i in value.columns) {
                    var column = value.columns[i];
                    column.title = column.title || (Commerce.ViewModelAdapter.getResourceString(column.titleResx) || "");
                    column.converter = column.converter || "";
                    column.cssClass = column.cssClass || "";

                    if (Commerce.StringExtensions.isNullOrWhitespace(column.defaultValue)) {
                        if (!Commerce.StringExtensions.isNullOrWhitespace(column.defaultValueResx)) {
                            column.defaultValue = Commerce.ViewModelAdapter.getResourceString(column.defaultValueResx);
                        } else {
                            column.defaultValue = "";
                        }
                    }
                    column.lines = column.lines || [];
                    column.type = column.type || Commerce.ListView.FieldType.text;

                    // For consistent way of passing object to converters swap to 'this' in WinJS grid.
                    if (column.field === "$data" && useWinJSListView) {
                        column.field = "this";
                    }
                }
            }

            var toggleEmptyGridTemplate = (count: number) => {
                var $tableContent = $element.find(".tableContent");
                var $listViewEmptyContentPlaceholder = $element.find(".listViewEmptyContentPlaceholder");
                if (count < 1) {
                    $tableContent.hide();
                    $tableContent.removeData(forceLayoutDone);
                    $listViewEmptyContentPlaceholder.css("display", "flex");
                    ko.applyBindingsToNode($listViewEmptyContentPlaceholder[0], {
                        template: {
                            name: value.emptyListViewTemplate,
                            data: viewModel
                        }
                    });
                } else {
                    if ($tableContent.is(':hidden')) {
                        $listViewEmptyContentPlaceholder.hide();
                        $tableContent.show();
                        var forceControlLayout = valueAccessor().forceControlLayout;
                        if (Commerce.ObjectExtensions.isNullOrUndefined(forceControlLayout)
                            && useWinJSListView
                            && (<any>$tableContent.data(forceLayoutDone)) !== true) {
                            listViewControl.forceLayout();
                            $tableContent.data(forceLayoutDone, true); // to avoid infinite loop we need marker to run only once. 
                        }
                    }
                }
            };

            if (useWinJSListView) {
                $element.addClass("col");
                var afterRenderHandler = (elementSet, bindingContext) => {
                    // WinJS list view will replicate elements with data-bind attributes and knockout will try to bind again.
                    cleanKnockoutBinding(elementSet[1]);
                };
                ko.applyBindingsToNode(element, { template: { name: "listViewStandardGridTemplate", data: value, afterRender: afterRenderHandler } });

                $element.find(".listViewHeader").addClass(value.rowClass);
                $element.find(".listViewLine").addClass(value.headerRowClass);

                // Create Template control.
                var $standardGridItemTemplate = $element.find(".standardGridItemTemplate");
                var standardGridItemTemplate = new WinJS.Binding.Template($standardGridItemTemplate[0]);

                // Create ListView control.
                var $listViewPlaceholder = $element.find(".listViewPlaceholder");
                var listViewControlOptions = {
                    itemTemplate: $standardGridItemTemplate[0],
                    layout: {
                        type: WinJS.UI.ListLayout
                    }
                };

                listViewControl = new WinJS.UI.ListView($listViewPlaceholder[0], listViewControlOptions);

                // Adds possibility for the user of the grid to update its layout when nessary
                // Usually it is nessarry when you need to hide and show the grid.
                var forceControlLayout = valueAccessor().forceControlLayout;
                if (forceControlLayout && ko.isObservable(forceControlLayout)) {
                    forceControlLayout(() => {
                        // Happens when we navigate to different page before the data is loaded.
                        if (listViewControl._disposed !== true) {
                            listViewControl.forceLayout();
                        }
                    });
                }

                var loadingStateChanged = (event: any) => {
                    // Set empty block if necessary.
                    if (listViewControl.loadingState === "complete" && value.emptyListViewTemplate) {
                        toggleEmptyGridTemplate(listViewControl.itemDataSource.list.length);
                    }
                };
                listViewControl.addEventListener("loadingstatechanged", WinJS.Utilities.markSupportedForProcessing(loadingStateChanged));

                // Attach additional handlers to the WinJS ListView.
                // Create copy of options sent to the control 
                // exclude the ones that winControl knockout handler does not support.
                var winControlBindings: any = {};
                for (var i in value) {
                    winControlBindings[i] = value[i];
                }

                // Fixes the swipe functionality for webkit browsers.
                if (Commerce.ObjectExtensions.isNullOrUndefined(winControlBindings.swipeBehavior)
                    || winControlBindings.swipeBehavior !== WinJS.UI.SwipeBehavior.none) {
                    winControlBindings.swipeBehavior = WinJS.UI.SwipeBehavior.select;
                }

                // Apply winControl knockout handler to the list view.
                ko.applyBindingsToNode($listViewPlaceholder[0], { winControl: winControlBindings }, viewModel);
            }
            else {

                $element.addClass("standardGrid listViewHandler");

                var afterRenderHandler = (elementSet: any, bindingContext: any) => {

                    var viewModelTemp = viewModel;
                    var selectedRowsKey = "selectedRows";
                    $element.data(selectedRowsKey, []);
                    var $elementSet: JQuery = $(elementSet);

                    // Parse each row and update elements that need additional options.
                    $elementSet.filter(".itemContainer").each((index, listViewLineElement) => {
                        var $listViewLineElement = $(listViewLineElement);
                        $listViewLineElement.find(".row").addClass(value.rowClass);

                        if (listViewLineElement.winControl) {
                            return;
                        }

                        // Create swipe element
                        var itemContainerOptions = {
                            swipeOrientation: "horizontal",
                            swipeBehavior: value.selectionMode === WinJS.UI.SelectionMode.none ? WinJS.UI.SwipeBehavior.none : WinJS.UI.SwipeBehavior.select
                        };

                        var itemContainer = new WinJS.UI.ItemContainer(<HTMLElement>listViewLineElement, itemContainerOptions);

                        // Attach oninvoked handler
                        var itemInvokeHandler = (eventArgs) => {
                            if (value.tapBehavior === "toggleSelect") {
                                itemContainer.selected = !itemContainer.selected;
                            }
                            if (value.iteminvoked) {
                                value.iteminvoked.call(viewModelTemp, <Commerce.ListView.IItemInvoked>{ currentTarget: eventArgs.currentTarget, data: bindingContext });
                            }
                        };
                        itemContainer.addEventListener("invoked", WinJS.Utilities.markSupportedForProcessing(itemInvokeHandler));

                        // Attach onselectionchanged handler
                        var selectionChangedHandler = (eventArgs) => {
                            var selectedRows: any[] = <any>$element.data(selectedRowsKey);
                            if (itemContainer.selected) {
                                selectedRows.push(bindingContext);

                                // Process single row selection mode.
                                if (value.selectionMode === WinJS.UI.SelectionMode.single) {
                                    $element.find(".itemContainer").each((index, listViewLineElement) => {
                                        if (listViewLineElement.winControl !== itemContainer) {
                                            listViewLineElement.winControl.selected = false;
                                        }
                                    });
                                }
                            } else {
                                var index = selectedRows.indexOf(bindingContext);
                                if (index > -1) {
                                    selectedRows.splice(index, 1);
                                }
                            }

                            if (value.selectionchanged) {
                                var appBarElementId: string = value.selectionchanged.appBarId;
                                var appBarControl = $(appBarElementId).get(0);

                                if (!Commerce.ObjectExtensions.isNullOrUndefined(appBarControl)) {
                                    if (selectedRows.length > 0) {
                                        appBarControl.winControl.show();
                                    } else {
                                        appBarControl.winControl.hide();
                                    }
                                }
                                value.selectionchanged.eventHandlerCallBack.call(viewModelTemp, selectedRows);
                            }
                        };
                        itemContainer.addEventListener("selectionchanged", WinJS.Utilities.markSupportedForProcessing(selectionChangedHandler));

                        // Expand button click handler.
                        $listViewLineElement.find(".expandButton").click((eventArgs) => {
                            var $expandButton = $(eventArgs.currentTarget);
                            var $itemContainer = $(eventArgs.currentTarget).closest(".itemContainer");
                            $itemContainer.toggleClass("expanded");

                            if ($itemContainer.hasClass("expanded")) {
                                $expandButton.removeClass("iconExpandSmall").addClass("iconCollapseSmall");
                            }
                            else {
                                $expandButton.removeClass("iconCollapseSmall").addClass("iconExpandSmall");
                            }

                            // Attach onexpand event handler.
                            var $colspanRow = $listViewLineElement.find(".colspanRow");
                            if (value.rowExpanded) {
                                value.rowExpanded.call(viewModelTemp, <Commerce.ListView.IItemExpand>{ colspanRow: $colspanRow.get(0), currentTarget: $colspanRow.closest(".itemContainer").get(0), data: bindingContext });
                            } else {
                                $colspanRow.hide();
                            }
                        });
                    });
                };

                value.dataRowsAfterRender = afterRenderHandler;

                // Make expand button click part of the template.
                ko.applyBindingsToNode(element, {
                    template: {
                        name: 'listViewEnhancedGridTemplate',
                        data: value,
                        afterRender: () => {
                            if (value.headerRowClass) {
                                $element.find(".listViewHeader").addClass(value.headerRowClass);
                            }

                            var scrollDiv = $element.find(".tableContent");
                            // Set empty block if necessary.
                            if (value.emptyListViewTemplate) {
                                if (value.itemDataSource) {
                                    // Toggle the empty grid template based on the length of an observable
                                    if (value.itemDataSource.subscribe) {
                                        value.itemDataSource.subscribe((newValue) => {
                                            toggleEmptyGridTemplate(newValue.length);
                                            if (value.scrollToBottom) {
                                                scrollDiv.scrollTop(scrollDiv[0].scrollHeight);
                                            }
                                        });

                                        toggleEmptyGridTemplate(value.itemDataSource().length);
                                    } else {
                                        // Toggle the empty grid template based on the length of a non-observable
                                        toggleEmptyGridTemplate(value.itemDataSource.length);
                                    }
                                } else {
                                    // Assume a length of 0 if the data source is not set
                                    toggleEmptyGridTemplate(0);
                                }
                            }
                        }
                    }
                }, this);

                return { controlsDescendantBindings: true };
            }
        }
    }
};
