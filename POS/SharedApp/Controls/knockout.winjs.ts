/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path='../Views/Controls/UserControl.ts'/>
/// <reference path='../Core/Converters.ts'/>
/// <reference path='../Core/Core.winjs.ts'/>
/// <reference path='../JQueryUI.d.ts'/>

ko.bindingHandlers.setImageOnError = {
    init: function (element: HTMLElement, valueAccessor: any) {
        element.addEventListener("error", () => {
            var value = ko.utils.unwrapObservable(valueAccessor() || {});
            Commerce.BindingHandlers.SetDefaultImageOnError(element, value);
        });
    }
};

ko.bindingHandlers.toggleSwitch = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var toggleSwitch = new WinJS.UI.ToggleSwitch(element);
        var value = ko.utils.unwrapObservable(valueAccessor() || {});

        for (var memberName in value) {
            switch (memberName) {
                case "checked":
                    // Set the event handler for the change event
                    var handler = (eventInfo) => {
                        if (typeof value.checked == "function") {
                            if (value.checked.subscribe) {
                                value.checked(eventInfo.currentTarget.winControl.checked);
                            }
                        }
                        else if (typeof value.checked == "boolean") {
                            value.checked = eventInfo.currentTarget.winControl.checked;
                        }
                    };
                    toggleSwitch.addEventListener("change", handler);

                    // Set the iniital state
                    if (typeof value.checked == "function") {
                        if (value.checked.subscribe) {
                            toggleSwitch.checked = value.checked();
                        }
                    }
                    else if (typeof value.checked == "boolean") {
                        toggleSwitch.checked = value.checked;
                    }
                    break;

                case "labelOn":
                    element.winControl.labelOn = Commerce.ViewModelAdapterWinJS.getResourceString(value[memberName]);
                    break;

                case "labelOff":
                    element.winControl.labelOff = Commerce.ViewModelAdapterWinJS.getResourceString(value[memberName]);
                    break;
            }
        }
    }
};

ko.bindingHandlers.resx = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // This will be called when the binding is first applied to an element
        // Set up any initial state, event handlers, etc. here

        var value = ko.utils.unwrapObservable(valueAccessor() || {});
        for (var memberName in value) {
            switch (memberName) {
                case "textContent":
                    setTextContent(Commerce.ViewModelAdapterWinJS.getResourceString(value[memberName]));
                    break;

                case "textWithTooltip":
                    var memberValue: string = Commerce.ViewModelAdapterWinJS.getResourceString(value[memberName]);
                    setTextContent(memberValue);
                    element.setAttribute("title", memberValue);
                    break;

                case "attribute":
                    var memberAttributes = ko.utils.unwrapObservable(value[memberName]);

                    for (var memberAttributeName in memberAttributes) {
                        element.setAttribute(memberAttributeName, Commerce.ViewModelAdapterWinJS.getResourceString(memberAttributes[memberAttributeName]));
                    }
                    break;

                case "label":
                    var memberValue:string = Commerce.ViewModelAdapterWinJS.getResourceString(value[memberName]);
                    element.winControl.label = memberValue;
                    setDefaultTooltip(memberValue);
                    break;

                case "labelOn":
                    element.winControl.labelOn = Commerce.ViewModelAdapterWinJS.getResourceString(value[memberName]);
                    break;

                case "labelOff":
                    element.winControl.labelOff = Commerce.ViewModelAdapterWinJS.getResourceString(value[memberName]);
                    break;

                case "title":
                    var memberValue: string = Commerce.ViewModelAdapterWinJS.getResourceString(value[memberName]);
                    element.winControl.title = memberValue;
                    setDefaultTooltip(memberValue);
                    break;

                case "tooltip":
                    element.winControl.tooltip = Commerce.ViewModelAdapterWinJS.getResourceString(value[memberName]);
                    break;

                case "ariaLabel":
                    element.setAttribute("aria-label", Commerce.ViewModelAdapterWinJS.getResourceString(value[memberName]));
                    break;
            }
        }

        function setDefaultTooltip(memberValue: string): void {
            // If tooltip property exists for the win control and if tooltip is not specified as part of resx, then set the value.
            if (("tooltip" in element.winControl) && !("tooltip" in value)) {
                element.winControl.tooltip = memberValue;
            }
        }

        function setTextContent(content: string) {
            element.textContent = content;
        }
    }
};

// Set focus on the dom elements which are hidden by default 
ko.bindingHandlers.hasfocus.update = function (element, valueAccessor) {
   var value = ko.utils.unwrapObservable(valueAccessor());
    setTimeout(function () {
        if (value
            && element.offsetWidth && element.offsetHeight
            && document.activeElement != element) {
                element.focus();
        }
    });
}

// Allows display of a menu control
// anchor (required): The element to anchor the flyout for display.
// placement: The placement of the flyout on the element. 
//            Possible values:
//            "auto", "top", "bottom", "left", or "right". The default is "auto".
// alignment: For top or bottom placement values, specifies alignment for left to right languages to the
//            center, left, or right.
//            Possible values:
//            "center", "left", or "right". The default is "center".
//            Remarks:
//            The alignment specified mirrors for right-to-left languages.
// toggleShowHide: An observable function parameter that will be set, so when called, will toggle the
//                 show / hide of the flyout control.
// hide: An observable function parameter that will be set, so when called, will hide the flyout control.
// show: An observable function parameter that will be set, so when called, will show the flyout control.
// Sample usage:
// menu: { anchor: '#titleSection', placement: 'bottom', alignment: 'left', toggleShowHide: _toggleShowHideMenu }
ko.bindingHandlers.menu = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // This will be called when the binding is first applied to an element
        // Set up any initial state, event handlers, etc. here

        var value = ko.utils.unwrapObservable(valueAccessor() || {});

        // Check the parameters
        if (Commerce.ObjectExtensions.isNullOrUndefined(value.anchor)) {
            throw new Error("The parameter anchor must be defined");
        }

        if (Commerce.ObjectExtensions.isNullOrUndefined(value.placement)) {
            value.placement = "auto";
        }

        if (Commerce.ObjectExtensions.isNullOrUndefined(value.alignment)) {
            value.alignment = "center";
        }

        if (!Commerce.ObjectExtensions.isNullOrUndefined(value.onAfterShowEvent)) {
            element.winControl.onaftershow = (event: any) => (value.onAfterShowEvent.call(viewModel, event));
        }

        // Support rtl
        if ((value.alignment == "right" || value.alignment == "left") && Commerce.CSSHelpers.isRightToLeft()) {
            value.alignment = (value.alignment == "right") ? "left" : "right";
        }

        var toggleShowHide = function () {
            if (element.winControl.hidden) {
                element.winControl.show($(value.anchor)[0], value.placement, value.alignment);
            } else {
                element.winControl.hide();
            }
        };

        if (!Commerce.ObjectExtensions.isNullOrUndefined(value.toggleShowHide)) {
            value.toggleShowHide(toggleShowHide);
        }

        var showFlyout = function () { if (element.winControl.hidden) { toggleShowHide(); }; };
        if (!Commerce.ObjectExtensions.isNullOrUndefined(value.show)) {
            value.show(showFlyout);
        }

        var hideFlyout = function () { if (!element.winControl.hidden) { toggleShowHide(); }; };
        if (!Commerce.ObjectExtensions.isNullOrUndefined(value.hide)) {
            value.hide(hideFlyout);
        }

        if (!Commerce.ObjectExtensions.isNullOrUndefined(value.sticky)) {
            var observableMember = value['sticky'];
            var memberValue = ko.utils.unwrapObservable(observableMember);
            if (typeof memberValue == "boolean") {
                element.winControl._sticky = memberValue;

                if (observableMember.subscribe) {
                    observableMember.subscribe(function (newValue) {
                        element.winControl._sticky = newValue;
                    });
                }
            }
        }
    }
};

// Allows display of a flyout control
// anchor (required): the element to anchor the flyout for display
// placement: the placement of the flyout on the element. 
//            Possible values:
//            "auto", "top", "bottom", "left", or "right".The default is "auto".
// alignment: For top or bottom placement values, specifies alignment for left to right languages to the
//            center, left, or right.
//            Possible values:
//            "center", "left", or "right".The default is "center".
//            Remarks:
//            The alignment specified mirrors for right-to-left languages.
// toggleShowHide: An observable function parameter that will be set, so when called, will toggle the
//                 show / hide of the flyout control.
// hide: An observable function parameter that will be set, so when called, will hide the flyout control
// show: An observable function parameter that will be set, so when called, will show the flyout control
// onAfterShowEvent: The function to be called after the flyout is shown
// Sample usage:
// flyout: { anchor: '#titleSection', placement: 'bottom', alignment: 'left', toggleShowHide: _toggleShowHideCurrencyCodeFlyout }
ko.bindingHandlers.flyout = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // This will be called when the binding is first applied to an element
        // Set up any initial state, event handlers, etc. here

        var value = ko.utils.unwrapObservable(valueAccessor() || {});

        // Check the parameters
        if (Commerce.ObjectExtensions.isNullOrUndefined(value.anchor)) {
            throw new Error("The parameter anchor must be defined");
        }

        if (Commerce.ObjectExtensions.isNullOrUndefined(value.placement)) {
            value.placement = "auto";
        }

        if (Commerce.ObjectExtensions.isNullOrUndefined(value.alignment)) {
            value.alignment = "center";
        }

        if (!Commerce.ObjectExtensions.isNullOrUndefined(value.onAfterShowEvent)) {
            element.winControl.onaftershow = (event: any) => (value.onAfterShowEvent.call(viewModel, event));
        }

        var toggleShowHide = function () {
            if (element.winControl.hidden) {

                var dynamicPlacement = $(value.anchor)[0].offsetTop > $("body").height() / 2 ? "top" : "bottom";
                element.winControl.show($(value.anchor)[0], dynamicPlacement, value.alignment);

            } else {
                element.winControl.hide();
            }
        };

        if (!Commerce.ObjectExtensions.isNullOrUndefined(value.toggleShowHide)) {
            value.toggleShowHide(toggleShowHide);
        }

        var showFlyout = function () { if (element.winControl.hidden) { toggleShowHide(); }; };
        if (!Commerce.ObjectExtensions.isNullOrUndefined(value.show)) {
            value.show(showFlyout);
        }

        var hideFlyout = function () { if (!element.winControl.hidden) { toggleShowHide(); }; };
        if (!Commerce.ObjectExtensions.isNullOrUndefined(value.hide)) {
            value.hide(hideFlyout);
        }

        if (!Commerce.ObjectExtensions.isNullOrUndefined(value.sticky)) {
            var observableMember = value['sticky'];
            var memberValue = ko.utils.unwrapObservable(observableMember);
            if (typeof memberValue == "boolean") {
                element.winControl._sticky = memberValue;

                if (observableMember.subscribe) {
                    observableMember.subscribe(function (newValue) {
                        element.winControl._sticky = newValue;
                    });
                }
            }
        }

        // The listview is not laid out if it is not visible. Force the layout of the containing listviews on showing the flyout to
        // show the listviews and allow the flyout to reize to the listview contents.
        element.winControl.addEventListener("beforeshow", function (event) {
            var listViewElements: any = element.querySelectorAll(".win-listview");
            for (var i: number = 0; i < listViewElements.length; i++) {
                listViewElements.item(i).winControl.recalculateItemPosition();
            }
        });
    }
};

// Allows modification of text controls
// select: Selects the text in the input control
//
// Sample usage:
// textInput: { select: _select }
ko.bindingHandlers.textInput = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // This will be called when the binding is first applied to an element
        // Set up any initial state, event handlers, etc. here

        var value = ko.utils.unwrapObservable(valueAccessor() || {});
        
        if (!Commerce.ObjectExtensions.isNullOrUndefined(value.select)) {
            var select = function () {
                element.select();
            };

            value.select(select);
        }
    }
};

ko.bindingHandlers.winControl = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // Separate long running process, do not wait for response.
        Commerce.Host.instance.timers.setImmediate(() => {
            // This will be called when the binding is first applied to an element
            // Set up any initial state, event handlers, etc. here

            var value = ko.utils.unwrapObservable(valueAccessor() || {});
            for (var memberName in value) {
                if (typeof memberName == "string") {
                    var observableMember = value[memberName];
                    var memberValue = ko.utils.unwrapObservable(observableMember);

                    switch (memberName) {
                        case "incrementalDataSource":
                            Commerce.ViewModelAdapterWinJS.createIncrementalDataSourceAdapter(
                                element.winControl,
                                memberValue.dataManager,
                                memberValue.callerMethod,
                                memberValue.pageSize,
                                memberValue.afterLoadComplete,
                                memberValue.onLoading,
                                memberValue.autoSelectFirstItem,
                                memberValue.pageLoadCallBack);
                            break;
                        case "itemDataSource":
                            var listDataSource = new WinJS.Binding.List(memberValue);
                            if (observableMember.subscribe) {
                                observableMember.subscribe(function (newValue: any[]) {
                                    WinJS.Promise.timeout().then(() => {
                                        listDataSource.splice(0, listDataSource.length);

                                        for (var i = 0; i < newValue.length; i++) {
                                            listDataSource.push(newValue[i]);
                                        }
                                    });
                                });
                            }

                            element.winControl.itemDataSource = listDataSource.dataSource;
                            break;
                        case "selectListViewItem":
                            if (memberValue.listViewId !== "") {
                                var listViewId = memberValue.listViewId;
                                element.winControl.addEventListener('pageselected', function (eventInfo) {
                                    var listViewControl = $(listViewId)[0];
                                    listViewControl.winControl.selection.clear();
                                    listViewControl.winControl.selection.add(element.winControl.currentPage);
                                    listViewControl.winControl.ensureVisible(element.winControl.currentPage);
                                });
                            }
                            break;

                        case "flipItem":
                            if (memberValue.flipViewId !== "") {
                                var flipViewId: string = memberValue.flipViewId;
                                var flipViewControl: any = $(flipViewId)[0].winControl;
                                element.winControl.addEventListener('selectionchanged', () => {
                                    if (element.winControl.selection.getIndices().length === 1) {
                                        flipViewControl.currentPage = element.winControl.selection.getIndices()[0];
                                    }
                                });
                            }
                            break;  
                        case "loadingstatechanged":
                            var memberValueTmp = memberValue;
                            var loadstatechangeHandlerAttached: string = "loadstatechangeHandlerAttached";
                            if (!Commerce.ObjectExtensions.isNullOrUndefined(memberValueTmp)) {
                                if (!Commerce.ObjectExtensions.isNullOrUndefined(memberValueTmp.AfterLoadComplete) &&
                                    Commerce.ObjectExtensions.isNullOrUndefined(element[loadstatechangeHandlerAttached]) &&
                                    !element[loadstatechangeHandlerAttached]) {

                                    element.winControl.addEventListener(memberName, function (eventInfo) {
                                        if (element.winControl.loadingState == "complete") {
                                            if (element.winControl.indexOfFirstVisible < 0
                                                && element.style.display != "none") {
                                                $(memberValueTmp.AfterLoadComplete).css("display", "flex");
                                                $(element).hide();
                                            } else {
                                                Commerce.RetailLogger.librariesWinJsListViewShown(element.id);

                                                // select the first item in the list view
                                                if (!Commerce.ObjectExtensions.isNullOrUndefined(memberValueTmp.AutoSelectFirstItem) &&
                                                    memberValueTmp.AutoSelectFirstItem &&
                                                    element.winControl.indexOfFirstVisible == 0 &&
                                                    element.winControl.selection.count() == 0) {
                                                    element.winControl.selection.set(0);
                                                }
                                            }
                                        } else if (element.winControl.loadingState == "viewPortLoaded") {
                                            if (!Commerce.ObjectExtensions.isNullOrUndefined(memberValueTmp.OnLoading) && memberValueTmp.OnLoading()) {
                                                memberValueTmp.OnLoading(false);
                                            }
                                        } else {
                                            $(memberValueTmp.AfterLoadComplete).hide();
                                            $(element).show();
                                        }
                                    });
                                    element[loadstatechangeHandlerAttached] = true;
                                }
                                else {
                                    if (!Commerce.ObjectExtensions.isNullOrUndefined(memberValueTmp.call)) {
                                        element.winControl.addEventListener(memberName, (eventInfo) => {
                                            memberValueTmp.call(viewModel, eventInfo);
                                        });
                                    }
                                }
                            }
                            break;                      
                        case "templateSwitch":
                            for (var i = 0; i < memberValue.length; i++) {
                                // memberValue[i].buttonId corresponds to the button Id to which the click handler is associated
                                // memberValue[i].templateId corresponds to the template id to be switch
                                // memberValue[i].layoutType corresponds to the layout of the view like WinJS.UI.GridLayout, WinJS.UI.ListLayout etc..
                                // memberValue[i].displayElementId corresponds to the Id of the element to be displayed on the page.

                                if (memberValue[i].buttonId !== "" && memberValue[i].templateId !== "" && memberValue[i].layoutType !== "" && memberValue[i].layoutType.type !== "") {

                                    var templateProperties = {
                                        buttonId: memberValue[i].buttonId,
                                        templateId: memberValue[i].templateId,
                                        layoutType: memberValue[i].layoutType.type,
                                        assignClassToId: memberValue[i].layoutType.assignClassToId,
                                        cssClasses: memberValue[i].layoutType.cssClasses,
                                        displayElementId: memberValue[i].displayElementId,
                                        appBarIds: memberValue[i].appBarIds
                                    };

                                    var setClickHandler = function (templateOptions, templateValues) {
                                        $(templateOptions.buttonId).click(function (event) {

                                            if (Commerce.ArrayExtensions.hasElements(templateOptions.appBarIds)) {
                                                for (var i = 0; i < templateOptions.appBarIds.length; i++) {
                                                    $(templateOptions.appBarIds[i])[0].winControl.hide();
                                                }
                                            }

                                            if ((!Commerce.ArrayExtensions.hasElements(element.winControl.itemTemplateId) && templateValues[0].templateId == templateOptions.templateId) ||
                                                element.winControl.itemTemplateId == templateOptions.templateId) {
                                                return;
                                            }

                                            $(templateOptions.assignClassToId).removeClass().addClass(templateOptions.cssClasses).promise().done(() => { 
                                                element.winControl.itemTemplateId = templateOptions.templateId;
                                                var templateElement = $(templateOptions.templateId)[0];
                                                if (element.winControl.setTemplate) {
                                                    element.winControl.setTemplate(templateElement);
                                                } else {
                                                    element.winControl.itemTemplate = templateElement;
                                                }
                                                element.winControl.layout = new templateOptions.layoutType();
                                                element.winControl.forceLayout(); // Chrome fix for resize recalculate issue with template switch views.
                                            });

                                            // Hide/show the elements linked to the templates when a template is selected
                                            for (var i = 0; i < templateValues.length; i++) {
                                                if (templateValues[i].displayElementId) {
                                                    var $displayElement = $(templateValues[i].displayElementId);

                                                    // Hide the element linked to the non-selected template
                                                    if (templateValues[i].templateId !== templateOptions.templateId) {
                                                        $displayElement.addClass("hide");
                                                    // Show the template linked to the selected template
                                                    } else {
                                                        $displayElement.removeClass("hide");
                                                    }
                                                }
                                            }
                                        });
                                    };

                                    setClickHandler(templateProperties, memberValue);
                                }
                            }
                            break;
                        case "groupedDataSource":
                            var memberValueTmp = memberValue;
                            var applyList = function (value) {
                                if (!value || value.length < 1) {
                                    return;
                                }

                                var elementRecordId = memberValueTmp.parentId();

                                // First level
                                var groupHeaderItems = [];
                                groupHeaderItems = value.filter((c, i, a) => {
                                    return (c[memberValueTmp.groupKeySelector] == elementRecordId || c[memberValueTmp.groupKeySelector] == c[memberValueTmp.groupDataSelector] );
                                });

                                groupHeaderItems = <Commerce.Model.Entities.Category[]>Commerce.ObjectExtensions.clone(groupHeaderItems)

                                // Second Level
                                var groupItems = [];
                                $.each(groupHeaderItems, function (index, groupHeaderItem) {

                                    var secondlevelItems = [];
                                    secondlevelItems = value.filter((c, i, a) => {
                                        return c[memberValueTmp.groupKeySelector] === groupHeaderItem[memberValueTmp.groupDataSelector];
                                    });

                                    groupItems.push.apply(groupItems, <Commerce.Model.Entities.Category[]>Commerce.ObjectExtensions.clone(secondlevelItems));

                                    groupHeaderItem[memberValueTmp.groupKeySelector] = groupHeaderItem[memberValueTmp.groupDataSelector];
                                });

                                groupHeaderItems.push.apply(groupHeaderItems, groupItems);

                                var listDataSource = new WinJS.Binding.List(groupHeaderItems);

                                var groupedDataSource = listDataSource.createGrouped(
                                    function groupKeySelector(item) {
                                        return item[memberValueTmp.groupKeySelector];
                                    },
                                    function groupDataSelector(item) {
                                        return { Name: item[memberValueTmp.keyName], SelectedGroupHeader: item };
                                    }
                                    );

                                element.winControl.itemDataSource = groupedDataSource.dataSource;
                                element.winControl.groupDataSource = groupedDataSource.groups.dataSource;
                            };

                            if (memberValue.itemList.subscribe) {
                                memberValue.itemList.subscribe(function (newValue) {
                                    applyList(newValue);
                                });
                            }
                            applyList(memberValue.itemList());

                            break;
                        // when the header item is invoked (clicked)
                        case "groupheaderinvoked":                            
                            element.winControl.addEventListener("groupheaderinvoked", function (eventInfo: CustomEvent) {
                                eventInfo.detail.groupHeaderPromise.then(function (headerItem: any) {
                                    memberValue.call(viewModel, headerItem.data);
                                });
                            });
                            break;
                        // Used for extending winjs 3.0.1 to enable swipe for chrome. However the swipe was depricated in 4.0 winJS 
                        // so we need to depricate swipe as well or support an ie browser for next versions of winJS.
                        case "swipeBehavior":
                            Commerce.Host.instance.application.registerSwipeBinding(element);
                            break;
                        case "iteminvoked":
                            var itemInvokedAttached: string = "itemInvokedAttached";
                            var itemInvokedCallback = memberValue;
                            if (Commerce.ObjectExtensions.isNullOrUndefined(element[itemInvokedAttached])) {
                                var newHandler = function (eventInfo) {
                                    Commerce.RetailLogger.librariesWinJsListViewItemClick(element.id);
                                    eventInfo.detail.itemPromise.then(function (item) {
                                        itemInvokedCallback.call(viewModel, item.data);
                                    });
                                };
                                element.winControl.addEventListener(memberName, newHandler);
                                element[itemInvokedAttached] = true;
                            }
                            break;
                        case "click":
                            var clickCallback = memberValue; // necessary to use another var to pass proper method.
                            element.winControl.addEventListener(memberName, function (eventInfo) {
                                clickCallback.call(viewModel, eventInfo);
                            });
                            break;
                        case "toggleChanged":
                            var changeCallback = memberValue; // necessary to use another var to pass proper method.
                            element.winControl.addEventListener("change", function (eventInfo) {
                                changeCallback.call(viewModel, eventInfo, eventInfo.currentTarget.winControl.checked);
                            });
                            break;
                        case "toggleOnOff":
                            if (typeof memberValue == "boolean") {
                                element.winControl.checked = memberValue;

                                if (observableMember.subscribe) {
                                    observableMember.subscribe(function (newValue) {
                                        element.winControl.checked = newValue;
                                    });
                                }
                            }
                            break;
                        case "selectionchanged":
                            var selectionChangedCallBack = memberValue; // necessary to use another var to pass proper method.
                            element.winControl.addEventListener(memberName, function (eventInfo) {
                                eventInfo.target.winControl.selection.getItems().then(function (item) {
                                    if (selectionChangedCallBack.appBarId != null
                                        && selectionChangedCallBack.appBarId != "") {
                                        if (eventInfo.target.winControl.selection.count() > 0) {
                                            $(selectionChangedCallBack.appBarId)[0].winControl.show();
                                        } else {
                                            $(selectionChangedCallBack.appBarId)[0].winControl.hide();
                                        }
                                    }
                                    selectionChangedCallBack.eventHandlerCallBack.call(viewModel, item.map(function (item) { return item.data; }));
                                });
                            });
                            break;
                        case "clearListViewSelection":
                            if (typeof memberValue == "boolean") {
                                if (observableMember.subscribe) {
                                    observableMember.subscribe(function (newValue) {
                                        if (newValue) {
                                            element.winControl.selection.clear();
                                        }
                                    });
                                }
                            }
                            break;
                        case "contentanimating":
                        case "keyboardnavigating":
                            // All other events not used for now and only have default implementation.
                            element.winControl.addEventListener(memberName, function (eventInfo) {
                                memberValue.call(viewModel, eventInfo);
                            });
                            break;
                        case "clearButton":
                            var setClearClickHandler = function (elementId) {
                                $(elementId).click(function (event) {
                                    element.winControl.selection.clear();
                                });
                            };

                            setClearClickHandler(memberValue);
                            break;
                        case "selectAllButton":
                            var setSelectAllClickHandler = function (elementId) {
                                $(elementId).click(function (event) {
                                    element.winControl.selection.selectAll();
                                });
                            };

                            setSelectAllClickHandler(memberValue);
                            break;
                        case "disabled":
                        case "enabled":
                            if (typeof memberValue == "boolean") {
                                var isForDisable: boolean = (memberName === "disabled");
                                element.winControl.disabled = isForDisable ? memberValue : !memberValue;

                                if (observableMember.subscribe) {
                                    observableMember.subscribe(function (newValue) {
                                        element.winControl.disabled = isForDisable ? newValue : !newValue;
                                    });
                                }
                            }
                            break;
                        case "visible":
                            if (typeof memberValue == "boolean") {
                                if (memberValue && element.winControl.show) {
                                    element.winControl.show();
                                } else if (element.winControl.hide) {
                                    element.winControl.hide();
                                }

                                if (observableMember.subscribe) {
                                    observableMember.subscribe(function (newValue) {
                                        if (typeof newValue == "boolean") {
                                            if (newValue === true && element.winControl.show) {
                                                element.winControl.show();
                                            } else if (element.winControl.hide) {
                                                element.winControl.hide();
                                            }
                                        }
                                    });
                                }
                            }
                            break;
                        case "forcelayout":
                                var forceLayout = (): void => {
                                    var disposed = element.winControl._disposed || false;
                                    if (!disposed) {
                                        element.winControl.forceLayout();
                                    }
                                };

                                observableMember(forceLayout);
                            break;
                        case "layout":
                            if (!Commerce.ObjectExtensions.isNullOrUndefined(memberValue.itemInfoMethod)) {
                                memberValue.itemInfo = (itemIndex: number): any => {
                                    return memberValue.itemInfoMethod.call(viewModel, itemIndex);
                                };
                            }

                            element.winControl.layout = memberValue;
                            break;
                        case "labelOn":
                            element.winControl.labelOn = Commerce.ViewModelAdapterWinJS.getResourceString(value[memberName]);
                            break;
                        case "labelOff":
                            element.winControl.labelOff = Commerce.ViewModelAdapterWinJS.getResourceString(value[memberName]);
                            break;
                        default:
                            element.winControl[memberName] = memberValue;
                            if (observableMember.subscribe) {
                                observableMember.subscribe(function (newValue) {
                                    element.winControl[memberName] = newValue;
                                });
                            }
                            break;
                    }
                }
            }
        });
    }
};

ko.bindingHandlers.formatPrice = {
    update: function (element, valueAccessor) {
        var value: number = ko.utils.unwrapObservable(valueAccessor());
        if (!Commerce.ObjectExtensions.isNumber(value)) {
            value = 0;
        }

        element.innerText = Commerce.NumberExtensions.formatCurrency(value);
    },
};

ko.bindingHandlers.formatPriceOrEmpty = {
    update: function (element: HTMLElement, valueAccessor) {
        var value: number = ko.utils.unwrapObservable(valueAccessor());
        if (!Commerce.ObjectExtensions.isNumber(value)) {
            element.innerText = Commerce.StringExtensions.EMPTY;
        } else {
            element.innerText = Commerce.NumberExtensions.formatCurrency(value);
        }
    },
};

ko.bindingHandlers.timestamp = {
    update: function (element, valueAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor()) || new Date();

        element.innerText = Commerce.StringExtensions.format(
            Commerce.ViewModelAdapter.getResourceString("string_1146"),
            Commerce.Host.instance.globalization.getDateTimeFormatter(Commerce.Host.Globalization.DateTimeFormat.SHORT_DATE).format(value),
            Commerce.Host.instance.globalization.getDateTimeFormatter(Commerce.Host.Globalization.DateTimeFormat.SHORT_TIME).format(value));
    },
};

ko.bindingHandlers.shortDate = {
    update: (element, valueAccessor) => {
        var value = ko.utils.unwrapObservable(valueAccessor());
        element.innerText = Commerce.Formatters.DateWithoutTime(value);
    }
};

ko.bindingHandlers.shortTime = {
    update: (element, valueAccessor) => {
        var value = ko.utils.unwrapObservable(valueAccessor());
        element.innerText = Commerce.Formatters.ShortTime(value);
    }
};

ko.bindingHandlers.enterKeyPress = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        ko.utils.registerEventHandler(element, "keypress", function (event: KeyboardEvent) {
            if (event.keyCode === 13) {
                event.preventDefault();
                event.stopImmediatePropagation();

                ko.utils.triggerEvent(element, "change");
                valueAccessor().call(viewModel);
            }
        });
    }
};

ko.bindingHandlers.navBarKeyboardSupport = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        ko.utils.registerEventHandler(element, "keydown", function (event: KeyboardEvent) {
            valueAccessor().keyValues.forEach((value) => {
                if (event.keyCode === value && $(element).is(":focus")) {
                    event.preventDefault();
                    event.stopImmediatePropagation();
                    ko.utils.triggerEvent(element, "change");
                    valueAccessor().handler.call(viewModel);
                }
            });
        });
    }
};

ko.bindingHandlers.showHideWithFocus = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var observable = valueAccessor();
        var $element = $(element);

        observable.subscribe((newValue) => {
            var slidedirection = !Commerce.CSSHelpers.isRightToLeft() ? "left" : "right";
            if (newValue) {
                $element.show("slide", { easing: 'easeInOutQuint', direction: slidedirection}, "fast", () => {
                    $(element).focus();
                });
            } else {
                $element.hide("slide", { easing: 'easeInOutQuint', direction: slidedirection }, "fast", () => {
                    $(element).blur();
                });
            }
        })
    }
}
ko.bindingHandlers.slider = {
    init: (element, valueAccessor, allBindingsAccessor): void => {
        var options = allBindingsAccessor().sliderOptions || {};
        var onSlide = allBindingsAccessor().onSlide || {};
        var $element = $(element);
        var sliderRangeValue = ko.utils.unwrapObservable(valueAccessor());

        options.slide = (event, selectedRangeValues) => {

            // Slider on JQuery UI does not yet support RTL. In this case we RTL-ing the range values instead
            // of the Slider itself.
            if (Commerce.CSSHelpers.isRightToLeft()) {
                var originalMin: number = selectedRangeValues.values[0];
                var originalMax: number = selectedRangeValues.values[1];
                selectedRangeValues.values[0] = options.max - (originalMax - options.min);
                selectedRangeValues.values[1] = options.max - (originalMin - options.min);
            }

            onSlide(event, selectedRangeValues);
        };
        
        $element.slider(options);
        $element.slider(sliderRangeValue.slice ? "values" : "value", sliderRangeValue);
        Commerce.UI.JQueryUITouchExtensions.enableTouchEmulation($element);

        //handle disposal (if KO removes binding)
        ko.utils.domNodeDisposal.addDisposeCallback(element, (): void => {
            Commerce.UI.JQueryUITouchExtensions.disableTouchEmulation($element);
        });
    }
};

// Makes elements shown/hidden via jQuery's fadeIn()/fadeOut() methods
ko.bindingHandlers.fadeVisible = {
    init: function (element, valueAccessor) {
        // Initially set the element to be instantly visible/hidden depending on the value
        var value = valueAccessor();
        $(element).toggle(ko.utils.unwrapObservable(value)); // Use "unwrapObservable" so we can handle values that may or may not be observable
    },
    update: function (element, valueAccessor, allBindingsAccessor) {
        // Whenever the value subsequently changes, fade the element in or out
        var value = valueAccessor();
        var fadeInTime = allBindingsAccessor().bindingOptions.fadeIn || 0;
        var fadeOutTime = allBindingsAccessor().bindingOptions.fadeOut || 0;
        var setfocusElement = allBindingsAccessor().setfocusElement || "";
        ko.utils.unwrapObservable(value) ? $(element).fadeIn(fadeInTime, () => {
            if (!Commerce.StringExtensions.isNullOrWhitespace(setfocusElement)) {
                $(element).find(setfocusElement).focus();
            }
        }) : $(element).fadeOut(fadeOutTime);
    }
};


// Toggle class binder method to create a highlight effect on change of the observable 
ko.bindingHandlers.toggleClass = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var value = valueAccessor();
        var className = allBindingsAccessor().bindingOptions.className || '';
        var updateCount = allBindingsAccessor().bindingOptions.updateCount || null;
        value.subscribe(function (newValue) {
            $(element).fadeOut('fast', 'easeInOutExpo', callback);
            function callback() {
                updateCount();
                $(element).addClass(className).fadeIn('slow', 'easeInOutExpo', removeClassCallBack);
                function removeClassCallBack() {
                    $(element).removeClass(className);
                }
            }
        });
    }
};

ko.bindingHandlers.customControlInternal = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var descendantBindingContext: any = $(element.parentElement).attr("descendantBindingContextApplied");

        // Bind the descendant elements on the first pass.
        // If a control is included, the binding will make two passes. One for the controls view source
        // and one for the containing dom's view source. This will only apply the view source bindings for the control.
        if ((descendantBindingContext == undefined) || (descendantBindingContext == null)) {
            descendantBindingContext = bindingContext;
            $(element.parentElement).attr("descendantBindingContextApplied", bindingContext);
            ko.applyBindingsToDescendants(bindingContext, element);
        }

        return { controlsDescendantBindings: true };
    }
};

// Assigns a DOM element which has the binding to a provided observable.
ko.bindingHandlers.setElementRef = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value : any = valueAccessor();
        if (ko.isObservable(value)) {
            value(element);
        }
    }
};

// Applies 'enable' binding for all child 'input' elements.
ko.bindingHandlers.enableAll = {
    update(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var enabled: boolean = ko.utils.unwrapObservable(valueAccessor());
        ko.bindingHandlers.enable.update(element, () => { return enabled; });

        ko.utils.arrayForEach(element.getElementsByTagName('input'), childElement => {
            ko.bindingHandlers.enable.update(childElement, () => { return enabled; });
        });
    }
};

module Commerce {
    "use strict";

    export class UserControlBindingHandler {
        public static init(element: HTMLElement, valueAccessor: Observable<any>, allBindingsAccessor: any, viewModel: any, bindingContext: any): any {
            var value = ko.utils.unwrapObservable(valueAccessor() || {});
            var uri: string = value.uri;
            var options: any = value.options || {};

            var pageControl = WinJS.UI.Pages.get(uri);
            var viewControllerType = pageControl.prototype._viewControllerType;

            var control: Commerce.Controls.UserControl;
            if (viewControllerType && Commerce.ObjectExtensions.isOfType(viewControllerType.prototype, Commerce.Controls.UserControl)) {
                // instantiates the control
                control = <Commerce.Controls.UserControl>new viewControllerType(options);

                // sets the control handle on the bindingContext.$data
                if (bindingContext.$data && options.controlHandle) {
                    bindingContext.$data[options.controlHandle] = control;
                }

                element.appendChild(control.element);
                control.render();
            } else {
                // Separate long running process, do not wait for response.
                Commerce.Host.instance.timers.setImmediate(() => {
                    WinJS.UI.Pages.render(uri, <HTMLElement>element.children[0], options);
                });
            }
        }
    }

    // The binding handler for a custom userControl should be last in the list of handlers as all windows binding should be
    // done first to keep this binding from overwriting the window's binding
    ko.bindingHandlers.userControl = Commerce.UserControlBindingHandler;
}