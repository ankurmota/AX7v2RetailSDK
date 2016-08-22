/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    export class ReportParameterTypeEnum {
        static String: string = "String";
        static Decimal: string = "Decimal";
        static Integer: string = "Integer";
        static DateTime: string = "DateTime";
    }

    export class ReportChartAttributesEnum {
        static Categories: string = "Categories";
        static Series: string = "Series";
    }

    export class ReportDetailsViewModel extends ViewModelBase {
        public Report: any;
        public ReportTitle: Observable<string>;
        public Parameters: ObservableArray<any>;
        public HasTotalRow: boolean;
        public HasDisclaimer: boolean;
        public ResultsCaption: Observable<string>;
        public Locale: string;
        public Output: any;
        public Charts: any;
        public ErrorInReport: Boolean;

        constructor(report: any) {
            super();
            this.Locale = Commerce.ApplicationContext.Instance.deviceConfiguration.CultureName;
            this.Report = report;
            this.ReportTitle = ko.observable(this.Report.REPORTTITLE);
            this.Parameters = ko.observableArray([]);
            this.HasTotalRow = false;
            this.HasDisclaimer = false;
            this.Charts = new Array();
            this.ResultsCaption = ko.observable("");
            this.SetParameters();
            this.SetCharts();
        }

        private SetParameters() {
            this.Parameters.removeAll();
            if (this.Report.PARAMETERS != null) {
                var parameters = this.Report.PARAMETERS.split("|");
                for (var i = 0; i < parameters.length; i++) {
                    if (parameters[i].length != 0) {
                            var parameter = parameters[i].split(";");
                            if(parameter.length > 0) {
                                var parameterName = ["", ""];
                                if (parameter.length > 0 && parameter[0].length > 0) {
                                    parameterName = parameter[0].split("=");
                                }
                               
                                var parameterType = ["", ""];
                                if (parameter.length > 1 && parameter[1].length > 0) {
                                    parameterType = parameter[1].split("=");
                                }
                                
                                var parameterLabel = ["", ""];
                                if (parameter.length > 2 && parameter[2].length > 0) {
                                    parameterLabel = parameter[2].split("=");
                                }
                                
                                var parameterValue = ["", ""];
                                if (parameter.length > 3 && parameter[3].length > 0) {
                                    parameterValue = parameter[3].split("=");
                                }

                                var todayDate = new Date();                        
                                switch (parameterType[1]) {
                                    case Commerce.ViewModels.ReportParameterTypeEnum.DateTime:
                                        var dateValue;
                                        // if the default value parameter contains 'today', then most likely used for the start date.
                                        // expecting this to be 'today - n' format. parse this and subtract from the default today's date value.
                                        // this is a hacky way to provide a default date value.
                                        if (parameterValue[1].toLocaleLowerCase().search('@today'.toLocaleLowerCase()) != -1) {
                                            var tokens = parameterValue[1].split('-');
                                            if (tokens.length == 1) {
                                                dateValue = todayDate;
                                            } else {
                                                var num = Number(tokens[1].trim());
                                                todayDate.setDate(todayDate.getDate() - num);
                                                dateValue = todayDate;
                                            }
                                        } else {
                                            dateValue = new Date(parameterValue[1]);
                                            if (isNaN(dateValue.getFullYear())) {
                                                dateValue = todayDate;
                                            }
                                        }
                                        var dateFormat = this.GetFormattedDate(dateValue);
                                        var dtParam = { "Name": parameterName[1], "Label": parameterLabel[1], "Value": dateFormat, "Type": parameterType[1] };
                                        this.Parameters.push(dtParam);
                                        break;
                                    case Commerce.ViewModels.ReportParameterTypeEnum.Integer:
                                        if (!jQuery.isNumeric(parameterValue[1])) {
                                            parameterValue[1] = "0";
                                        }

                                        var intParam = { "Name": parameterName[1], "Label": parameterLabel[1], "Value": NumberExtensions.formatNumber(NumberExtensions.parseNumber(parameterValue[1].toString()), NumberExtensions.getDecimalPrecision()), "Type": parameterType[1] };
                                        this.Parameters.push(intParam);
                                        break;
                                    case Commerce.ViewModels.ReportParameterTypeEnum.Decimal:
                                        if (!jQuery.isNumeric(parameterValue[1])) {
                                            parameterValue[1] = "0.0";
                                        }

                                        var decParam = { "Name": parameterName[1], "Label": parameterLabel[1], "Value": NumberExtensions.formatNumber(NumberExtensions.parseNumber(parameterValue[1].toString()), NumberExtensions.getDecimalPrecision()), "Type": parameterType[1] };
                                        this.Parameters.push(decParam);
                                        break;
                                    default:
                                        var dfParam = { "Name": parameterName[1], "Label": parameterLabel[1], "Value": parameterValue[1], "Type": parameterType[1] };
                                        this.Parameters.push(dfParam);
                                        break;
                            }
                        }
                    }
                }
            }
        }

        private SetCharts() {
            this.Charts = [];
            if (this.Report.CHARTS != null) {
                var charts = this.Report.CHARTS.split("|");
                for (var i = 0; i < charts.length; i++) {
                    if (charts[i].length != 0) {
                        var chart = charts[i].split(";");
                        var categories = {};
                        var series = [];
                        for (var j = 0; j < chart.length; j++) {
                            var chartAttribute = chart[j].split("=");
                            switch (chartAttribute[0]) {
                                case Commerce.ViewModels.ReportChartAttributesEnum.Categories:
                                    categories = chartAttribute[1];
                                    break;
                                case Commerce.ViewModels.ReportChartAttributesEnum.Series:
                                    series.push(chartAttribute[1]);
                                    break;
                            }
                        }
                        this.Charts.push({ "Categories": categories, "Series": series });
                    }
                }
            }
        }

        private GetFormattedDate(date: Date): string {
            //This date is sent to Retail server to get the data.
            return date.getFullYear().toString() + "/" + ('0' + (date.getMonth() + 1)).slice(-2) + "/" + ('0' + date.getDate()).slice(-2);
        }

        /**
         * Gets the parameter string used when getting the report data and set the results caption used by the views to show report query data.
         */
        private GetParameters(): Model.Entities.CommerceProperty[] {
            var parameters = [];
            this.ErrorInReport = false;
            var resultCaption = "";
            var paramCaptionTexts: string[] = [];
            var paramCaptionTextFormat = Commerce.ViewModelAdapter.getResourceString("string_2407");

            for (var i = 0; i < this.Parameters().length; i++) {
                var parameterProperty = new Model.Entities.CommercePropertyClass();
                var value = this.Parameters()[i].Value;
                parameterProperty.Key = this.Parameters()[i].Name;
                parameterProperty.Value = new Model.Entities.CommercePropertyValueClass();
                var paramCaptionText: string = null;
                switch (this.Parameters()[i].Type) {
                    case Commerce.ViewModels.ReportParameterTypeEnum.DateTime:
                        var date = new Date(value.toString());
                        if (isNaN(date.getFullYear())) {
                            this.ErrorInReport = true;
                        } else {
                            parameterProperty.Value.DateTimeOffsetValue = date;
                            var formattedDate: string = Host.instance.globalization.getDateTimeFormatter(Host.Globalization.DateTimeFormat.SHORT_DATE).format(date);
                            paramCaptionText = StringExtensions.format(paramCaptionTextFormat, this.Parameters()[i].Label, formattedDate);
                        }

                        break;
                    case Commerce.ViewModels.ReportParameterTypeEnum.Decimal:
                        var showValue = value.toString();
                        parameterProperty.Value.DecimalValue = NumberExtensions.parseNumber(value.toString());
                        if (!jQuery.isNumeric(value)) {
                            this.ErrorInReport = true;
                        }

                        paramCaptionText = StringExtensions.format(paramCaptionTextFormat, this.Parameters()[i].Label, showValue);
                        break;
                    case Commerce.ViewModels.ReportParameterTypeEnum.Integer:
                        var showValue = value.toString();
                        parameterProperty.Value.DecimalValue = NumberExtensions.parseNumber(value.toString());
                        if (!jQuery.isNumeric(value)) {
                            this.ErrorInReport = true;
                        }

                        paramCaptionText = StringExtensions.format(paramCaptionTextFormat, this.Parameters()[i].Label, showValue);
                        break;
                    default:
                        parameterProperty.Value.StringValue = value;
                        paramCaptionText = StringExtensions.format(paramCaptionTextFormat, this.Parameters()[i].Label, value);
                        break;
                }

                if (paramCaptionText != null) {
                    paramCaptionTexts.push(paramCaptionText);
                }

                
                parameters.push(parameterProperty);
            }

            if (!this.ErrorInReport) {
                var paramCaptionTextSeperator = Commerce.ViewModelAdapter.getResourceString("string_2408");

                // Build the result caption by adding all the parameter strings and the seperator except the last parameter string
                for (var paramCaptionTextIndex: number = 0; paramCaptionTextIndex < paramCaptionTexts.length - 1; paramCaptionTextIndex++) {
                    resultCaption += paramCaptionTexts[paramCaptionTextIndex] + paramCaptionTextSeperator;
                }

                // Add the last parameter string to the result caption
                if (paramCaptionTexts.length > 0) {
                    resultCaption += paramCaptionTexts[paramCaptionTextIndex];
                }

                this.ResultsCaption(resultCaption);
            } else {
                this.ResultsCaption(Commerce.ViewModelAdapter.getResourceString("string_2404"));
            }

            return parameters;
        }

        public GetReportDataFromParameters(): IVoidAsyncResult {
            var parameters = this.GetParameters();
            if (!this.ErrorInReport) {
                return this.reportManager.getReportsDataAsync(this.Report.REPORTID, parameters, this.Locale)
                    .done((result) => {
                        this.Output = result.Output;
                        this.HasTotalRow = result.HasTotalRow;
                        this.HasDisclaimer = result.HasDisclaimer;
                    })
                    .recoverOnFailure((errors: Model.Entities.Error[]): IVoidAsyncResult => {
                        return VoidAsyncResult.createRejected([new Model.Entities.Error("string_2405")]);
                    });
            }

            return VoidAsyncResult.createRejected([new Model.Entities.Error("string_2404")]);
        }
    }
}
