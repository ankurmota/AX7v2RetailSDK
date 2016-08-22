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

    export module SingleSelectRefinerControl {

        export interface ISingleSelectRefinerControlOptions {
            Refiner: Proxy.Entities.ProductRefiner;
            SelectedRefinerValues: Observable<Proxy.Entities.ProductRefinerValue[]>;
        }
    }
}

/*
    SingleSelectRefinerControl knockout binding extension
*/
ko.bindingHandlers.SingleSelectRefinerControl = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value: Commerce.SingleSelectRefinerControl.ISingleSelectRefinerControlOptions = ko.utils.unwrapObservable(valueAccessor()) || {};
        var $element = $(element);

        var refiner: Commerce.Proxy.Entities.ProductRefiner = <Commerce.Proxy.Entities.ProductRefiner>Commerce.ObjectExtensions.clone(value.Refiner);

        var customMaxValue: Observable<string> = ko.observable(null);
        var customMinValue: Observable<string> = ko.observable(null);
        
        if (refiner.DisplayTemplateValue == 3) {

            var customRefinerValue = new Commerce.Proxy.Entities.ProductRefinerValueClass();
            customRefinerValue.DataTypeValue = refiner.DataTypeValue;
            customRefinerValue.LeftValueBoundString = customMinValue();
            customRefinerValue.RightValueBoundString = customMaxValue();
            customRefinerValue.UnitText = null;
            customRefinerValue.RefinerRecordId = refiner.RecordId;
            refiner.Values.push(customRefinerValue);
            customMaxValue.subscribe((value) => {
                customRefinerValue.RightValueBoundString = customMaxValue();
            })

            customMinValue.subscribe((value) => {
                customRefinerValue.LeftValueBoundString = customMinValue();
            })
        }

        var optionString = (data: Commerce.Proxy.Entities.ProductRefinerValue): string => {
            if (refiner.DisplayTemplateValue == 3) {
                if (data.LeftValueBoundString == null && data.RightValueBoundString == null) {
                    return "Custom";
                } else {
                    return data.LeftValueBoundString + '-' + data.RightValueBoundString;
                }
            } else {
                return data.LeftValueBoundString;
            }
        }

        var isCustomRangeVisible: Observable<boolean> = ko.observable(false);


        var selectedOptions: Observable<Commerce.Proxy.Entities.ProductRefinerValue> = ko.observable({});
        var optionsCaption: string = Commerce.StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_39"), refiner.KeyName.toLocaleLowerCase());

        var afterRefinerUIRender = () => {
            $element.find("select")[0].focus();
        };

        ko.applyBindingsToNode(element, {
            template: {
                name: "singleSelectRefinerControlTemplate",
                data: {
                    values: refiner,
                    selectedOptions: selectedOptions,
                    optionsCaption: optionsCaption,
                    optionString: optionString,
                    isCustomRangeVisible: isCustomRangeVisible,
                    customMaxValue: customMaxValue,
                    customMinValue: customMinValue
            }, afterRender: afterRefinerUIRender
            }
        });

        selectedOptions.subscribe((item) => {
            var selectedItems: Commerce.Proxy.Entities.ProductRefinerValue[] = [];

            if (!Commerce.ObjectExtensions.isNullOrUndefined(item)) {
                if (item.LeftValueBoundString == null && item.RightValueBoundString == null) {
                    isCustomRangeVisible(true);
                } else {
                    isCustomRangeVisible(false);
                }                
                selectedItems.push(item);
            }

            if (!Commerce.ObjectExtensions.isNullOrUndefined(value.SelectedRefinerValues)) {
                value.SelectedRefinerValues(selectedItems);
            }
        });
    }
};
