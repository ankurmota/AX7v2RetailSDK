/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Core/Core.winjs.ts'/>

/*
 *  tabControl knockout binding extension
 *
 *
 * tabControl binding usage example:
 * tabControl: { 
 *          data: transactionPageTabControl1,
 *          tabClick: tabClick,
 *          visible: true,
 *          callbackContext: someContext
 *        }
 *
 * @param {function} tabClick Function that gets excecuted with each tab click.
 * @param {object} data Instance of tab control.
 * @param {boolean} visible Determines if buttongrid is visible or not.
 * @param {object} callbackContext Context to pass to tabClick callback.
 */


module Commerce.Controls.Tab {
    export interface ITabControl {
        /* Zero-based index of the selected tab. This could also be an observable. */
        selectedIndex: any;

        /* Placement of tabs */
        tabsPosition: number;

        /* Array of tab items */
        tabItems: ITabItem[];

        /* Control width */
        width: number;
    }

    /* Used internally to store information relevant to the control */
    export class TabOptions {
        tabClick: (index: number) => void;
        callbackContext: any;
        id: string;
        data: ITabControl; // TO DO: ultimately, this will be of type TabControl defined in Retail Server
        $element: JQuery;
        $tabContent: JQuery;
        $tabsContainer: JQuery;
    }

    /* Position of the tabs relative to the control */
    export enum TabsPosition {
        Left = 0,
        // Top = 1, // TO DO: Posponed on [zalin]'s instruction.
        Right = 2,
        // Bottom = 3 // TO DO: Posponed on [zalin]'s instruction.
    }

    /* Tab item interface */
    export interface ITabItem {
        /*  The identifier of element to show when this tab item is selected. 
            This element is hidden when this tab item is not selected. 
        */
        associatedElementId: string;

        /*  The display text on the tab item. */
        displayText: string;

        /*  Additional CSS classes to be applied to this tab item at runtime. */
        cssClasses: string;

        /*  Boolean indicating if this is the selected tab item or not. */
        isSelected: boolean;

        /*  Tab icon */
        pictureAsBase64: string;

        /*  Boolean that determines if this tab item is visible or not. */
        visible: boolean;
    }
}

ko.bindingHandlers.tabControl = (function () {
    "use strict";

    var _optionsKey = "tabControlOptions";
    var _tabOptionsKey = "tabOptions";
    var _accentColorCssClass = "accent";
    var _controlCssClass = "commerceTabControl";

    /* This function handles clicks from tabControl tabs and invokes the tabClick callback on the view model/controller if one was provided */
    var tabClick = function (e: JQueryEventObject): void {
        e.stopPropagation();

        var $tab = $(e.currentTarget);
        var $element = $tab.closest('.' + _controlCssClass);
        // retrieve options
        var options: Commerce.Controls.Tab.TabOptions = <any>$element.data(_optionsKey);
        // retrieve tab data
        var tab: Commerce.Controls.Tab.ITabItem = <any>$tab.data(_tabOptionsKey);

        if (options == null || tab == null) return;

        var newIndex = options.data.tabItems.indexOf(tab);
        if (options.data.selectedIndex && options.data.selectedIndex.subscribe) {
            // no need explicitly call update ad the changing observable will trigger the same.
            options.data.selectedIndex(newIndex);
        } else {
            options.data.selectedIndex = newIndex;
            updateDOM(options);
        }

        if (typeof (options.tabClick) == 'function') {
            options.tabClick.call(options.callbackContext, options.data.selectedIndex);
        }
    };

    /* 
    * Update the DOM with current control state. 
    * 
    * @param {boolean} visible Set to true to render the control, false to hide the control.
    * @param {TabOptions} options Options required to translate the current state of the tab control to the UI.
    */
    var updateDOM = function (options: Commerce.Controls.Tab.TabOptions): void {
        // default selection
        if (Commerce.ObjectExtensions.isNullOrUndefined(options.data.selectedIndex)) {
            options.data.selectedIndex = 0;
        }

        // tabs
        if (options.data.tabItems) {
            var tabs: Commerce.Controls.Tab.ITabItem[] = options.data.tabItems;
            if (tabs != null) {
                // clear tabs
                options.$tabsContainer.empty();

                // create tabs
                for (var i = 0; i < tabs.length; i++) {
                    var tab: Commerce.Controls.Tab.ITabItem = tabs[i];
                    tab.isSelected = i === ko.utils.unwrapObservable(options.data.selectedIndex);

                    var $associatedElement = $('#' + tab.associatedElementId);
                    if (tab.visible == null || tab.visible) {
                        var $tab = $('<button class="tab tabBackgroundColor"><div class="icon"></div><div class="text semilight">' +
                            tab.displayText + '</div><div class="indicator"></div></button>');
                        $tab.data(_tabOptionsKey, tab);
                        // position associated element in content area
                        options.$tabContent.append($associatedElement);

                        if (tab.isSelected) {
                            $tab.addClass("selected");
                            $tab.find('.indicator').addClass(_accentColorCssClass + 'Background');

                            // show associated DOM element
                            $associatedElement.show();
                        } else {
                            // hide associated DOM element
                            $associatedElement.hide();
                        }

                        // image, applied to button background
                        if (!Commerce.StringExtensions.isNullOrWhitespace(tab.pictureAsBase64)) {
                            $tab.find(".icon").css("background-image", "url('data:image;base64," + tab.pictureAsBase64 + "')");
                        }

                        if (tab.cssClasses) {
                            $tab.addClass(tab.cssClasses);
                        }

                        options.$tabsContainer.append($tab);
                        $tab.attr("data-ax-bubble", "tab_" + tab.displayText.toLowerCase());

                        $tab.click(tabClick);
                    } else {
                        //hide associated DOM element
                        $associatedElement.hide();
                    }
                }
            }
        }
    };

    return {
        init: function (element: Element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {

            var value: any = ko.utils.unwrapObservable(valueAccessor()) || {};
            var $element = $(element);
            $element.addClass(_controlCssClass); // control root CSS class

            // id attribute must be set on the control
            var id = value.id || $element.attr('id');
            if (!id) {
                Commerce.ViewModelAdapter.displayMessage("tab control requires a unique Id", Commerce.MessageType.Error);
                return;
            }
            var item = null;
            if (value.view) {
                item = Commerce.ApplicationContext.Instance.tillLayoutProxy.getLayoutItem(value.view, id);
                if (!Commerce.ObjectExtensions.isNullOrUndefined(item)) {

                    var tabData = <Commerce.Controls.Tab.ITabControl>{};
                    tabData.tabsPosition = Number(item.TabStripPlacement);
                    tabData.tabItems = [];

                    item.TabPages.forEach((tabPage) => {
                        var tabItem = <Commerce.Controls.Tab.ITabItem>{};

                        tabItem.displayText = tabPage.Title;
                        var imageZones = Commerce.ApplicationContext.Instance.tillLayoutProxy.getImageZones([tabPage.ZoneID]);

                        if (Commerce.ArrayExtensions.hasElements(imageZones)) {
                            tabItem.pictureAsBase64 = imageZones[0].PictureAsBase64;
                        }

                        tabItem.associatedElementId = tabPage.Content.ID;
                        tabData.tabItems[tabData.tabItems.length] = tabItem;
                    });
                }

                value.data = tabData;

                var orientationChangedHandler = (args) => {
                    if (item) {
                        // Move elements back to its parent.
                        item.TabPages.forEach((tabPage) => {
                            var $control = $element.find("#" + tabPage.Content.ID);
                            $control.removeAttr("style"); // Remove styles in case where display none is set by the tab.
                            $element.parent().parent().append($control);
                        });
                    }

                    Commerce.ApplicationContext.Instance.tillLayoutProxy.removeOrientationChangedHandler(element, orientationChangedHandler);

                    // Clear tab control content before re-bind
                    $element.empty();
                    // Re-bind the control with new orientation
                    ko.applyBindings(viewModel, element);
                };

                Commerce.ApplicationContext.Instance.tillLayoutProxy.addOrientationChangedHandler(element, orientationChangedHandler);
            }

            // initialize options 
            var options: Commerce.Controls.Tab.TabOptions = {
                callbackContext: value.callbackContext || bindingContext,
                tabClick: value.tabClick,
                id: id,
                data: value.data,
                $element: $element,
                $tabContent: null,
                $tabsContainer: null,
            };

            // tab content
            var $tabContent = $('<div class="tabContent"></div>');
            $tabContent.addClass("col grow");
            options.$tabContent = $tabContent;
            $element.append($tabContent);

            // tabs container
            var $tabsContainer = $('<div class="tabsContainer"></div>');
            $tabsContainer.addClass("col");
            options.$tabsContainer = $tabsContainer;
            $element.append($tabsContainer);

            $element.addClass("row");

            if (options.data) {
                switch (options.data.tabsPosition) {
                    case Commerce.Controls.Tab.TabsPosition.Left:
                        $element.addClass("lefttabs");
                        $element.removeClass("righttabs");
                        break;

                    case Commerce.Controls.Tab.TabsPosition.Right:
                    default:
                        $element.addClass("righttabs");
                        $element.removeClass("lefttabs");
                        break;
                }

                $element.data(_optionsKey, options);
            }

            return { controlsDescendantBindings: true };
        },

        update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            var $element = $(element);
            var value: any = ko.utils.unwrapObservable(valueAccessor()) || {};

            var tabControl: Commerce.Controls.Tab.ITabControl = <Commerce.Controls.Tab.ITabControl>value.data;
            if (Commerce.ObjectExtensions.isNullOrUndefined(tabControl)) return;

            var options: Commerce.Controls.Tab.TabOptions = <any>$element.data(_optionsKey);
            if (Commerce.ObjectExtensions.isNullOrUndefined(options) ||
                Commerce.ObjectExtensions.isNullOrUndefined(options.id)) {
                return;
            }

            // update data, especially to support changing tabs
            options.data = value.data;

            updateDOM(options);
        }
    };

})();
