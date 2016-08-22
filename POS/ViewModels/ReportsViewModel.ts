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

    export class ReportsViewModel extends ViewModelBase {
        public ReportsList: ObservableArray<any>;
        private Locale: string;

        constructor(options: any) {
            super();
            this.ReportsList = ko.observableArray([]);
            this.Locale = Commerce.ApplicationContext.Instance.deviceConfiguration.CultureName;
            this.reportManager.getListOfReportsAsync(this.Locale)
                .done((reportDataSet) => { this.getListOfReportsSuccessCallBack(reportDataSet); })
                .fail((errors) => { this.getListOfReportsErrorCallBack(); });
        }

        private getListOfReportsSuccessCallBack(data: Model.Entities.ReportDataSet) {
            var list = new Array();
            var row = {};

            for (var i = 0; i < data.Output.length; i++) {
                var currentRow = data.Output[i];
                row = {};
                if (currentRow.RowData != null) {
                    for (var j = 0; j < currentRow.RowData.length; j++) {
                        row[currentRow.RowData[j].Key] = currentRow.RowData[j].Value.StringValue;
                    }

                    list.push(row);
                }
            }

            this.ReportsList(list);
        }

        private getListOfReportsErrorCallBack() {
            Commerce.ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString("string_2405"), Commerce.MessageType.Error);
        }
    }
}