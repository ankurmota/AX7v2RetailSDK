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
    export module SliderRefinerControl {
        export interface ISliderRefinerControlOptions {
            Refiner: Proxy.Entities.ProductRefiner;
            SelectedRefinerValues: Observable<Proxy.Entities.ProductRefinerValue[]>;
        }
    }
}

/*
    SliderRefinerControl knockout binding extension
*/
ko.bindingHandlers.sliderRefinerControl = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var value: Commerce.SliderRefinerControl.ISliderRefinerControlOptions = ko.utils.unwrapObservable(valueAccessor()) || {};
        var $element = $(element);

        // number array used to databind the selected range.
        var originalRangeValues: ObservableArray<number>;
        var formattedRangeValues: string[];
        var originalRefinerValue: Commerce.Proxy.Entities.ProductRefinerValue;
        var currentRange: Observable<string>;
        var rangeFormat: string = "&nbsp;({0}&nbsp;&ndash;&nbsp;{1})";

        if (!Commerce.ObjectExtensions.isNullOrUndefined(value.Refiner) && Commerce.ArrayExtensions.hasElements(value.Refiner.Values)) {
            originalRefinerValue = value.Refiner.Values[0];

            // Parses string range values and converts into number range values.
            var getRangeValues = (currentRefinerValue: Commerce.Proxy.Entities.ProductRefinerValue): number[] => {
                var convertedLeftValueBound: number = Math.floor(parseFloat(currentRefinerValue.LeftValueBoundString));
                var convertedRightValueBound: number = Math.ceil(parseFloat(currentRefinerValue.RightValueBoundString));

                return [convertedLeftValueBound, convertedRightValueBound];
            };

            // If the refiner is of type currency, then currency format is applied and returns the formatted range values. 
            // If not currency, then just converts to string values without applying any formatting.
            var getFormattedRangeValues =
                (currentRangeValues: number[], dataTypeValue: Commerce.Proxy.Entities.AttributeDataType, unitText: string): string[] => {
                    var formattedLeftValueBoundString: string = currentRangeValues[0].toString();
                    var formattedRightValueBoundString: string = currentRangeValues[1].toString();

                    if (dataTypeValue === Commerce.Proxy.Entities.AttributeDataType.Currency) {
                        formattedLeftValueBoundString = Commerce.NumberExtensions.formatCurrency(currentRangeValues[0], unitText);
                        formattedRightValueBoundString = Commerce.NumberExtensions.formatCurrency(currentRangeValues[1], unitText);
                    }

                    return [formattedLeftValueBoundString, formattedRightValueBoundString];
                };

            originalRangeValues = ko.observableArray(getRangeValues(originalRefinerValue));
            formattedRangeValues = getFormattedRangeValues(originalRangeValues(), originalRefinerValue.DataTypeValue, originalRefinerValue.UnitText);
            currentRange = ko.observable(Commerce.StringExtensions.format(rangeFormat, formattedRangeValues[0], formattedRangeValues[1]));

            var onSlide = (event, selectedRangeValues: { values: number[] }) => {
                var currentRefineValue: Commerce.Proxy.Entities.ProductRefinerValue =
                    <Commerce.Proxy.Entities.ProductRefinerValue>Commerce.ObjectExtensions.clone(originalRefinerValue);

                currentRefineValue.LeftValueBoundString = selectedRangeValues.values[0].toString();
                currentRefineValue.RightValueBoundString = selectedRangeValues.values[1].toString();

                var formattedCurrentRangeValues: string[] = getFormattedRangeValues(selectedRangeValues.values, currentRefineValue.DataTypeValue, currentRefineValue.UnitText);
                currentRange(Commerce.StringExtensions.format(rangeFormat, formattedCurrentRangeValues[0], formattedCurrentRangeValues[1]));

                if (!Commerce.ObjectExtensions.isNullOrUndefined(value.SelectedRefinerValues)) {
                    value.SelectedRefinerValues([currentRefineValue]);
                }
            };

            var afterRefinerUIRender = () => {
                $element.find(".slider")[0].focus();
            };

            ko.applyBindingsToNode(element, {
                template: {
                    name: "sliderRefinerControlTemplate", data: {
                        refiner: value.Refiner,
                        currentRange: currentRange,
                        rangeValues: originalRangeValues,
                        options: {
                            min: originalRangeValues()[0],
                            max: originalRangeValues()[1],
                            range: true
                        },
                        formattedMin: formattedRangeValues[0],
                        formattedMax: formattedRangeValues[1],
                        slide: onSlide
                    }, afterRender: afterRefinerUIRender
                }
            });
        }


    }
};
