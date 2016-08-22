/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../../HighCharts.src.d.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";
    
    export class TotalColumn {
        cssClass: string;
        value: string;
        title: string;
    }

    export class ReportResultsViewController extends ViewControllerBase {
        private ViewModel: Commerce.ViewModels.ReportResultsViewModel;
        public commonHeaderData;
        public ParsedChartsData: any;
        public Titles: any;
        public Footers: ObservableArray<any>;
        public Content: ObservableArray<any>;
        public DivNamesArray: any;
        public GridControlVisible: Observable<boolean>;
        public ChartVisible: ObservableArray<boolean>;
        private stepVal: number;
        public summaryReportOptions: Observable<Commerce.ListView.IListViewOptions>;
        public isTotalsEnabled: Observable<boolean>;
        public totalColumns: ObservableArray<TotalColumn>;
        public disclaimer: string;
        public isDisclaimerDisplayed: boolean;

        constructor(options?: any) {
            super(true);

            this.Footers = ko.observableArray([]);
            this.Content = ko.observableArray([]);
            this.GridControlVisible = ko.observable(false);
            this.ChartVisible = ko.observableArray([false, false, false]);
            this.ParsedChartsData = new Array();
            this.Titles = new Array();
            this.ViewModel = new Commerce.ViewModels.ReportResultsViewModel(options);
            this.DivNamesArray = ["chartPlaceHolder1", "chartPlaceHolder2", "chartPlaceHolder3"];
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.summaryReportOptions = ko.observable(null);
            this.isTotalsEnabled = ko.observable(false);
            this.totalColumns = ko.observableArray<TotalColumn>([]);
            this.disclaimer = Commerce.ViewModelAdapter.getResourceString("string_2402");
            this.isDisclaimerDisplayed = this.ViewModel.HasDisclaimer;

            // Load Common Header
            this.commonHeaderData.viewSectionInfo(false);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.categoryName(this.ViewModel.ReportTitle());
            // Set report title based on connection status online/offline
            if (Commerce.Session.instance.connectionStatus == ConnectionStatusType.Online) {
                this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_2409")); // Reports
            } else {
                this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_2410")); // Reports
            }
        }

        /**
         * Called when the page is loaded on the DOM.
         */
        public load() {
            this.CreateHighCharts();
        }

        public CreateHighCharts() {
            if (this.ViewModel.Output.length > 0) {
                this.CreateReportsData();
                this.LoadHighChart();
            }
        }


        /**
          * Creates and sets the report data used for the summary display and the individual charts
          */
        private CreateReportsData() {
            // Create and set the report data used by the high charts and the summary report
            if (this.ViewModel.Output.length > 0) {
                this.GridControlVisible = ko.observable(true);
                this.stepVal = Math.floor(this.ViewModel.Output.length / 7) + 1;

                for (var j = 0; j < this.ViewModel.Output[0].RowData.length; j++) {
                    var rowCol = this.ViewModel.Output[0].RowData[j];
                    var series = { "name": rowCol.Key, "data": new Array() };
                    this.ParsedChartsData.push(series);
                    this.Titles.push(rowCol.Key);
                }
                
                // Loop through the rows
                for (var i: number = 0; i < this.ViewModel.Output.length; i++) {

                    // Get the data for the rows if the row has data
                    if (ArrayExtensions.hasElements(this.ViewModel.Output[i].RowData)) {
                        var values: any = {};

                        // Loop through the cells in the row by column
                        for (var j: number = 0; j < this.Titles.length; j++) {

                            // Get the data for the cell
                            var chartValue = this.ViewModel.ReturnCommercePropertyValue(this.ViewModel.Output[i].RowData[j].Value);

                            // if this dataset does not have a total row, push value
                            // if this dataset does have total row, but we are not at last row, push value
                            if (!this.ViewModel.HasTotalRow || i != this.ViewModel.Output.length - 1) {
                                this.ParsedChartsData[j]["data"].push(chartValue);
                            }

                            // Get the cell value for display in the summary report
                            var currentValue: any = chartValue;
                            var isNumber: boolean = false;
                            if (j != 0 && typeof (currentValue) === "number") {
                                currentValue = NumberExtensions.formatNumber(currentValue, NumberExtensions.getDecimalPrecision());
                                isNumber = true;
                            }

                            values["Data" + j.toString()] = currentValue;
                            values["DataIsNumber" + j.toString()] = isNumber;
                        }

                        this.Content.push(values);
                    }
                }
            }

            this.isTotalsEnabled(this.ViewModel.HasTotalRow);

            //
            // Set the summary report data for display
            //
            var summaryContentData: any[] = this.Content();
            var totalRow;
            if (ArrayExtensions.hasElements(summaryContentData)) {
                totalRow = summaryContentData[summaryContentData.length - 1]; //last row.
            }
            // Create the columns for the summary data using the first data row to indicate column type
            var summaryColumns: Commerce.ListView.IListViewOptionColumn[] = [];
            for (var j: number = 0; j < this.Titles.length; j++) {

                var cssClass: string = "width25 ellipsis";
                if (!ObjectExtensions.isNullOrUndefined(totalRow)) {
                    if (totalRow["DataIsNumber" + j.toString()]) {
                        cssClass = "width15 ellipsis textRight";
                    }
                }

                var summaryColumn: Commerce.ListView.IListViewOptionColumn = {
                    title: this.Titles[j],
                    cssClass: cssClass,
                    field: "Data" + j.toString()
                };
                
                if (this.ViewModel.HasTotalRow) {
                    var val = totalRow["DataIsNumber" + j.toString()] ? totalRow["Data" + j.toString()] : "";
                    var title = totalRow["DataIsNumber" + j.toString()] ? this.Titles[j] : "";
                    this.totalColumns.push({ cssClass: cssClass, value: val, title: title });
                }

                summaryColumns.push(summaryColumn);
            }

            if (this.isTotalsEnabled()) {
                this.Content.remove(this.Content()[this.Content().length - 1]);
            }

            // Create the IListOptions object for the summary report data
            var summaryReportOptions: Commerce.ListView.IListViewOptions = {
                itemDataSource: this.Content,
                selectionMode: WinJS.UI.SelectionMode.none,
                tapBehavior: 'none',
                columns: summaryColumns
            };

            this.summaryReportOptions(summaryReportOptions);
        }

        private LoadHighChart() {
            var categories = new Array();
            var series;

            for (var i = 0; i < this.ViewModel.Charts.length; i++) {
                if (i >= 3) {
                    break;
                }
                var cindex = jQuery.inArray(this.ViewModel.Charts[i]["Categories"], this.Titles);
                if (cindex > -1) {
                    categories = (this.ParsedChartsData[cindex].data);
                }

                series = new Array();
                for (var j = 0; j < this.ViewModel.Charts[i]["Series"].length; j++) {
                    var sindex = jQuery.inArray(this.ViewModel.Charts[i]["Series"][j], this.Titles);
                    if (sindex > -1) {
                        series.push(this.ParsedChartsData[sindex]);
                    }
                }
                if (i <= this.DivNamesArray.length) {
                    this.createChart(categories, series, this.DivNamesArray[i]);
                }
                this.ChartVisible[i] = true;
            }

        }

        private createChart(categories, series, renderDivName) {
            var accentColor = CSSHelpers.accentColor();
            var tooltipText = Commerce.ViewModelAdapter.getResourceString("string_2407");
            var fontName: string = "Segoi UI";
            var lineColor: string = "#999999";
            var fontColor: string = "#b7b7b7";
            var options: any = {
                colors: [accentColor, "#B1E3F9", "#1098D2", "#0A658C", "#8BD6F6", "#074156", "#D8F1FC"],
                chart: {
                    renderTo: document.getElementById(renderDivName),
                    type: 'column',
                    backgroundColor: 'transparent'
                },
                title: {
                    text: '',
                    style: {
                        color: fontColor,
                        font: fontName
                    },
                    useHTML: true
                },
                xAxis: {
                    categories: categories,
                    labels: {
                        step: this.stepVal,
                        rotation: 0,
                        align: 'center',
                        style: {
                            color: fontColor,
                            font: fontName,
                            fontSize: '9px',
                            width: '50px'
                        },
                        formatter: function() {
                            return this.value;
                        }
                    },
                    lineColor: lineColor,
                    lineWidth: 1
                },
                yAxis: {
                    min: 0,
                    title: {
                        text: '',
                        style: {
                            color: fontColor,
                            font: fontName,
                            fontSize: '9px'
                        }
                    },
                    allowDecimals: false
                },
                legend: {
                    itemStyle: {
                        color: fontColor,
                        font: fontName
                    }
                },
                tooltip: {
                    backgroundColor: "#000000",
                    borderWidth: 0,
                    style: {
                        color: '#FFF',
                        font: fontName
                    },
                    formatter: function() {
                        return '<b>' + this.x + '</b><br/>' +
                            StringExtensions.format(tooltipText, this.series.name, NumberExtensions.formatNumber(this.y, NumberExtensions.getDecimalPrecision()));
                    }
                },
                series: series
            };

            if (Host.instance.application.getBrowserType() === Commerce.Host.BrowserType.Chrome) {
                let isRtl = CSSHelpers.getDeveloperModeTextDirection() === CSSHelpers.RIGHT_TO_LEFT_TEXT_DIRECTION;
                options.tooltip.rtl = isRtl;
                options.legend.rtl = isRtl;
                options.legend.textAlign = "left";
            }

            var chart = new Highcharts.Chart(options);
        }
    }
}
