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
    export module MultiSelectRefinerControl {
        export interface IMultiSelectRefinerControlOptions {
            Refiner: Proxy.Entities.ProductRefiner;
            SelectedRefinerValues: Observable<Proxy.Entities.ProductRefinerValue[]>;
        }
    }
}

/*
    MultiSelectRefinerControl knockout binding extension
*/
ko.bindingHandlers.MultiSelectRefinerControl = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value: Commerce.MultiSelectRefinerControl.IMultiSelectRefinerControlOptions = ko.utils.unwrapObservable(valueAccessor()) || {};
        var $element = $(element);
        var selectedOptions: ObservableArray<Commerce.Proxy.Entities.ProductRefinerValue> = ko.observableArray([]);   
        var flyout: WinJS.UI.Flyout;
        var checkedOptions: ObservableArray<any> = ko.observableArray([]);

        var $body = $("body");

        var okButtonHandler = () => {
            var selectedValues: Commerce.Proxy.Entities.ProductRefinerValue[] = [];

            checkedOptions().forEach((option) => {
                var filteredRefinerValue = value.Refiner.Values.filter((item) => {
                    return option === item.LeftValueBoundString;
                });

                if (Commerce.ArrayExtensions.hasElements(filteredRefinerValue)) {
                    selectedValues.push(filteredRefinerValue[0]);
                }
            });

            selectedOptions(selectedValues);
            if (!Commerce.ObjectExtensions.isNullOrUndefined(value.SelectedRefinerValues)) {
                value.SelectedRefinerValues(selectedValues);
            }

            flyout.hide();
        };

        var onSelectedButtonHandler = (data, eventInfo) => {
            checkedOptions(checkedOptions().filter((option) => {
                return option !== data.LeftValueBoundString;
            }));

            var selectedValues: Commerce.Proxy.Entities.ProductRefinerValue[] = [];
            checkedOptions().forEach((option) => {
                var filteredRefinerValue: Commerce.Proxy.Entities.ProductRefinerValue[] = [];
                filteredRefinerValue = value.Refiner.Values.filter((item) => {
                    return option === item.LeftValueBoundString;
                });
                if (Commerce.ArrayExtensions.hasElements(filteredRefinerValue)) {
                    selectedValues.push(filteredRefinerValue[0]);
                }
            });

            selectedOptions(selectedValues);
            if (!Commerce.ObjectExtensions.isNullOrUndefined(value.SelectedRefinerValues)) {
                value.SelectedRefinerValues(selectedValues);
            }

            eventInfo.stopPropagation();
        };
        var cancelButtonHandler = () => {
            flyout.hide();
        };

        var afterRefinerUIRender = () => {
            var flyoutContainer = $element.find(".refinerFlyout").get(0);
            var flyoutAlignment = (!Commerce.CSSHelpers.isRightToLeft()) ? "right" : "left";
            flyout = new WinJS.UI.Flyout(flyoutContainer, { anchor: $element.find(".topAnchor").get(0), placement: "bottom", alignment: flyoutAlignment });
        };

        var addButtonHandler = (data, eventInfo) => {
            if (flyout.hidden) {
                var bodyHeight = $body.height();
                var evaluatedHeight = (bodyHeight / 2 - 0.2 * bodyHeight);
                var $refinerButton: JQuery = $element.find(".refinerAddButton");

                var isTopPlacement: boolean = $refinerButton[0].offsetTop > evaluatedHeight;

                if (isTopPlacement) {
                    flyout.placement = "top";
                    flyout.show($element.find(".bottomAnchor").get(0));
                } else {
                    flyout.placement = "bottom";
                    flyout.show($element.find(".topAnchor").get(0));
                }
            } else {
                flyout.hide();
            }
        };

        ko.applyBindingsToNode(element, {
            template: {
                name: "multiSelectRefinerControlTemplate", data: {
                    values: selectedOptions,
                    addButton: addButtonHandler,
                    refiner: value.Refiner,
                    checkedOptions: checkedOptions,
                    okButtonHandler: okButtonHandler,
                    cancelButtonHandler: cancelButtonHandler,
                    onSelectedButtonClick: onSelectedButtonHandler
                }, afterRender: afterRefinerUIRender
            }
        });        
    }
};
